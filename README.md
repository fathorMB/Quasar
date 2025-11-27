# Quasar Framework

> A modular, event-driven application framework for .NET 10.

Quasar is a comprehensive framework designed for building scalable, modular monoliths and microservices. It provides a robust foundation for Event Sourcing, CQRS, Identity Management, and modern Frontend integration with extensible custom UI support.

## 🚀 Key Features

- **Modular Architecture**: Built on Clean Architecture principles with loose coupling via `Quasar.Core` and `Quasar.Cqrs`.
- **Event Sourcing**: First-class support for Event Sourcing with multiple providers:
  - **SQLite**: For development and embedded scenarios.
  - **SQL Server**: For production-grade persistence.
  - **InMemory**: For testing.
- **Identity & Access Management**: Complete system with Users, Roles, Permissions, and JWT Authentication.
- **Network Discovery**: Built-in UDP discovery service for automatic network visibility.
- **Modern Frontend**: Integrated **React 19** + **Vite 7** UI shell with **dynamic custom UI loading**.
- **Real-time**: SignalR integration for live updates.
- **Background Processing**: Quartz.NET integration for scheduled tasks.
- **Observability**: Telemetry and structured logging.

## 📦 Package Ecosystem

The framework is organized into granular packages:

### Core & Infrastructure
| Package | Description |
|---------|-------------|
| `Quasar.Core` | Core abstractions, utilities, and result types. |
| `Quasar.Cqrs` | Command and Query bus interfaces and implementations. |
| `Quasar.Domain` | DDD base classes (Aggregates, Entities, Value Objects). |
| `Quasar.Infrastructure` | Cross-cutting infrastructure concerns. |
| `Quasar.Logging` | Structured logging configuration. |
| `Quasar.Security` | Security utilities and encryption. |
| `Quasar.Web` | ASP.NET Core integration and UI configuration. |

### Feature Modules
| Package | Description |
|---------|-------------|
| `Quasar.Identity` | Identity domain (Users, Roles, Auth). |
| `Quasar.Discovery` | UDP network discovery service. |
| `Quasar.Ui.React` | Modern React frontend shell application. |

### Persistence & Event Sourcing
| Package | Description |
|---------|-------------|
| `Quasar.EventSourcing` | Core Event Sourcing abstractions. |
| `Quasar.EventSourcing.Sqlite` | SQLite event store implementation. |
| `Quasar.EventSourcing.SqlServer` | SQL Server event store implementation. |
| `Quasar.Projections` | Read model projection engine. |
| `Quasar.Persistence.Relational.EfCore` | EF Core repository patterns. |
| `Quasar.Persistence.Document.Mongo` | MongoDB document storage. |

## 🛠️ Getting Started

### Prerequisites
- **.NET 10.0 SDK**
- **Node.js 20+** (for UI development)

### Quick Start

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourorg/quasar.git
   cd quasar
   ```

2. **Build the UI Shell:**
   ```bash
   cd src/Quasar.Ui.React
   npm install
   npm run build
   cd ../..
   ```

3. **Run your application:**
   ```bash
   cd src/YourApp
   dotnet run
   ```

4. **Access the UI:**
   Open your browser to the configured port (default: https://localhost:7160)

   **Default Credentials:**
   - **Admin**: `admin` / `ChangeMe123!`
   - **Operator**: `operator` / `ChangeMe123!`

## ⚙️ Configuration

### Network Discovery
Enable UDP discovery in `Program.cs` to make your service discoverable:

```csharp
services.AddQuasarDiscovery(options =>
{
    options.Enabled = true;
    options.Port = 6000; // Default: 5353
    options.ServiceName = "My Quasar Service";
    options.Metadata = new Dictionary<string, string>
    {
        { "version", "1.0.0" },
        { "env", "production" }
    };
});
```

### UI Configuration

Configure the React UI shell:

```csharp
// In Program.cs
services.AddQuasarUi(ui =>
{
    ui.ApplicationName = "My Application";
    ui.Theme = "dark"; // 'dark', 'orange', 'green'
    ui.LogoSymbol = "M"; // Letter shown in sidebar
    ui.CustomBundleUrl = "/myapp-ui/myapp-ui.js"; // Optional: load custom UI
});

// Map framework UI endpoints
app.MapQuasarUiEndpoints();

// Serve the React UI (SPA fallback)
app.UseQuasarReactUi();
```

## 🎨 Building Custom UI Extensions

Quasar allows you to extend the UI shell with application-specific pages and routes. The framework provides the administrative UI (Users, Roles, Features, etc.), while your application provides domain-specific pages.

### Architecture

- **Framework Shell** (`Quasar.Ui.React`): Provides authentication, layout, and administrative pages
- **Custom UI Bundle** (`YourApp.Ui.React`): Your application-specific React components loaded dynamically

### Creating a Custom UI Project

#### 1. Create the Project

```bash
cd src
npm create vite@latest YourApp.Ui.React -- --template react-ts
cd YourApp.Ui.React
```

#### 2. Install Dependencies

**Critical**: Match these versions exactly with the framework shell:

```bash
npm install react@^19.2.0 react-dom@^19.2.0 react-router-dom@^7.9.6 axios@^1.13.2
```

#### 3. Configure Vite for UMD Library Build

Update `vite.config.ts`:

```typescript
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

export default defineConfig({
  plugins: [react()],
  build: {
    lib: {
      entry: path.resolve(__dirname, "src/myapp.ts"),
      name: "MyAppUi",
      formats: ["umd"], // Important: UMD format for side effects
      fileName: () => "myapp-ui.js",
    },
    outDir: path.resolve(__dirname, "dist"),
    emptyOutDir: true,
    rollupOptions: {
      // Externalize dependencies provided by the shell
      external: ["react", "react-dom", "react-router-dom"],
      output: {
        globals: {
          react: "React",
          "react-dom": "ReactDOM",
          "react-router-dom": "ReactRouterDOM",
        },
      },
    },
  },
});
```

#### 4. Create Type Definitions

`src/types.ts`:

```typescript
import type { FC } from 'react';

export interface CustomNavItem {
  label: string;
  path: string;
  roles?: string[]; // Restrict to specific roles
  feature?: string; // Require feature flag
}

export interface CustomNavSection {
  title: string;
  items: CustomNavItem[];
}

export interface CustomRoute {
  path: string;
  component: FC;
  index?: boolean;
  roles?: string[];
  feature?: string;
}
```

#### 5. Create Entry Point

`src/myapp.ts`:

```typescript
import { DashboardPage } from "./pages/DashboardPage";
import type { CustomNavSection, CustomRoute } from "./types";

declare global {
  interface Window {
    __QUASAR_CUSTOM_MENU__?: CustomNavSection[];
    __QUASAR_CUSTOM_ROUTES__?: CustomRoute[];
  }
}

// Register your custom menu
window.__QUASAR_CUSTOM_MENU__ = [
  {
    title: "My Application",
    items: [
      { label: "Dashboard", path: "/" },
      { label: "Reports", path: "/reports" },
      { label: "Admin Panel", path: "/admin", roles: ["administrator"] }
    ],
  },
];

// Register your custom routes
window.__QUASAR_CUSTOM_ROUTES__ = [
  {
    path: "/",
    component: DashboardPage,
    index: true,
  },
  {
    path: "/reports",
    component: ReportsPage,
  },
  {
    path: "/admin",
    component: AdminPage,
    roles: ["administrator"],
  },
];
```

#### 6. Create Your Pages

`src/pages/DashboardPage.tsx`:

```typescript
import React, { useEffect, useState } from 'react';
import { myApi } from '../api/myapi';

export const DashboardPage: React.FC = () => {
  const [data, setData] = useState([]);

  useEffect(() => {
    myApi.list().then(setData);
  }, []);

  return (
    <div className="page-container">
      <div className="page-header">
        <h1>Dashboard</h1>
        <p className="text-muted">Welcome to My Application</p>
      </div>
      
      <div className="card">
        <h2>My Data</h2>
        {/* Your content here */}
      </div>
    </div>
  );
};
```

#### 7. Create API Client

`src/api/myapi.ts`:

```typescript
import axios from 'axios';

export interface MyModel {
  id: string;
  name: string;
}

export const myApi = {
  async list(): Promise<MyModel[]> {
    const response = await axios.get('/api/myendpoint');
    return response.data;
  },
};
```

#### 8. Configure Your .NET Application

In your application's `Program.cs`:

```csharp
// Configure UI to load your custom bundle
services.AddQuasarUi(ui =>
{
    ui.ApplicationName = "My Application";
    ui.Theme = "dark";
    ui.LogoSymbol = "M";
    ui.CustomBundleUrl = "/myapp-ui/myapp-ui.js"
});
```

#### 9. Build and Deploy

Add to your `.csproj`:

```xml
<PropertyGroup>
  <SpaRoot>..\YourApp.Ui.React\</SpaRoot>
</PropertyGroup>

<Target Name="BuildCustomUi" BeforeTargets="Build">
  <Exec Command="npm install" WorkingDirectory="$(SpaRoot)" />
  <Exec Command="npm run build" WorkingDirectory="$(SpaRoot)" />
</Target>

<Target Name="PublishCustomUi" AfterTargets="Publish">
  <ItemGroup>
    <CustomUiFiles Include="$(SpaRoot)dist\**\*" />
  </ItemGroup>
  <Copy SourceFiles="@(CustomUiFiles)" 
        DestinationFolder="$(PublishDir)wwwroot\myapp-ui\%(RecursiveDir)" />
</Target>
```

Or manually:

```bash
cd YourApp.Ui.React
npm run build
xcopy /Y dist\*.js ..\YourApp\wwwroot\myapp-ui\
```

### Custom UI Project Structure

```
YourApp.Ui.React/
├── src/
│   ├── api/              # API clients for your backend
│   │   └── myapi.ts
│   ├── pages/            # Your application pages
│   │   ├── DashboardPage.tsx
│   │   └── ReportsPage.tsx
│   ├── myapp.ts          # Entry point (registers routes/menu)
│   └── types.ts          # Type definitions
├── package.json
├── vite.config.ts
└── tsconfig.json
```

**What NOT to include:**
- ❌ Framework pages (Users, Roles, Features, etc.)
- ❌ Authentication logic
- ❌ App.tsx, main.tsx (you're building a library, not a standalone app)
- ❌ Layout components

### Available CSS Classes

The framework shell provides these classes for your custom pages:

- `.page-container` - Main page wrapper
- `.page-header` - Page header section
- `.card` - Card container
- `.btn`, `.btn-primary`, `.btn-secondary` - Buttons
- `.text-muted` - Muted text
- CSS variables for spacing, colors, etc.

### Menu Ordering

By default, custom menu sections appear **before** the administrative section. The order is:

1. Your custom sections (e.g., "My Application", "Devices")
2. Administration (Users, Roles, Features, etc.) - for administrators only

## 🐛 Troubleshooting

### Black Screen After Login

**Symptom**: Page loads but shows black screen after authentication.

**Common Causes:**
1. Custom bundle URL not configured in `UiSettings.CustomBundleUrl`
2. React version mismatch between shell and custom UI
3. Bundle not built as UMD format
4. Dependencies not externalized in vite.config.ts

**Solution:**
```bash
# Verify bundle exists
ls YourApp/wwwroot/myapp-ui/

# Check browser console for errors
# Verify: window.__QUASAR_CUSTOM_ROUTES__ should be defined

# Rebuild with correct config
cd YourApp.Ui.React
npm run build
```

### Module Not Found Errors

Ensure your custom UI project only includes application-specific code. Framework functionality is provided by the shell.

## 📚 Additional Resources

- [API Documentation](docs/API.md)
- [Architecture Guide](docs/ARCHITECTURE.md)
- [Custom UI Developer Guide](src/QUASAR_DEVELOPER_GUIDE.md)

## 🤝 Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
