# Quasar Framework

> A modular, event-driven application framework for .NET 10.

Quasar is a comprehensive framework designed for building scalable, modular monoliths and microservices. It provides a robust foundation for Event Sourcing, CQRS, Identity Management, and modern Frontend integration.

## 🚀 Key Features

- **Modular Architecture**: Built on Clean Architecture principles with loose coupling via `Quasar.Core` and `Quasar.Cqrs`.
- **Event Sourcing**: First-class support for Event Sourcing with multiple providers:
  - **SQLite**: For development and embedded scenarios.
  - **SQL Server**: For production-grade persistence.
  - **InMemory**: For testing.
- **Identity & Access Management**: Complete system with Users, Roles, Permissions, and JWT Authentication.
- **Network Discovery**: Built-in UDP discovery service for automatic network visibility.
- **Modern Frontend**: Integrated **React 19** + **Vite 7** UI, served directly from ASP.NET Core.
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
| `Quasar.Ui.React` | Modern React frontend application. |

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

### Running the Reference App (BEAM)

The `BEAM.App` project is the reference implementation showcasing all framework features.

1.  **Navigate to the project:**
    ```bash
    cd src/BEAM.App
    ```

2.  **Run the application:**
    ```bash
    dotnet run
    ```
    *This will automatically seed the database and start the React frontend.*

3.  **Access the UI:**
    Open [http://localhost:5288](http://localhost:5288) in your browser.

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
Configure the React UI integration:

```csharp
// In Program.cs
services.AddQuasarUi(ui =>
{
    ui.ApplicationName = "My App";
    ui.Theme = "dark"; // 'dark', 'orange', 'green'
});

// ...
app.UseQuasarReactUi();
```

## 🤝 Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
