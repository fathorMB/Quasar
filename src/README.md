# Quasar Micro-Framework

Quasar is an opinionated .NET 8 micro-framework for building data-centric, CQRS and event-sourced APIs. It combines mediator-driven command/query handling, a pluggable event store, relational/document read models, identity & ACL, logging, and optional real-time streaming over SignalR with TimescaleDB time-series persistence.

## Table of Contents

- [Highlights](#highlights)
- [Architecture Overview](#architecture-overview)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Clone & Build](#clone--build)
  - [Run the Sample API](#run-the-sample-api)
- [Core Packages](#core-packages)
  - [Quasar.Core](#quasarcore)
  - [Quasar.Cqrs](#quasarcqrs)
  - [Quasar.Domain](#quasardomain)
  - [Quasar.EventSourcing](#quasareventsourcing)
  - [Quasar.Persistence](#quasarpersistence)
  - [Quasar.Security](#quasarsecurity)
  - [Quasar.Logging](#quasarlogging)
  - [Quasar.RealTime](#quasarrealtime)
- [Identity & ACL](#identity--acl)
- [Time-Series & SignalR Streaming](#time-series--signalr-streaming)
- [Sample Web UI](#sample-web-ui)
- [Extending Quasar](#extending-quasar)
- [Testing](#testing)
- [License](#license)

## Highlights

- **Mediator-centric CQRS** with pipeline behaviors for validation, transactions, and authorization.
- **Event-sourced aggregates** with pluggable stores (SQL Server, SQLite, in-memory).
- **Relational read models** powered by EF Core and optional document/time-series abstractions.
- **Identity & ACL** with JWT issuance, refresh tokens, and dynamic role/permission management.
- **Serilog logging** helpers for console, file, and Seq sinks.
- **Real-time streaming** via SignalR with automatic TimescaleDB persistence.
- **Fully wired sample API & Web UI** to experiment with all features (counter/cart commands, sensor telemetry).

## Architecture Overview

```
┌────────────────┐    ┌──────────────┐
│  API Endpoints │───▶│   Mediator   │──┐
└────────────────┘    └──────────────┘  │
                                         │
                                 ┌───────▼────────┐
                                 │  Pipeline      │
                                 │  Behaviors     │
                                 │ (Validation,   │
                                 │  Transaction,  │
                                 │  Authorization)│
                                 └───────┬────────┘
                                         │
                              ┌──────────▼─────────┐
                              │ Command/Query      │
                              │ Handlers           │
                              └──────────┬─────────┘
                                         │
                  ┌──────────────────────▼──────────────────────┐
                  │   Event Sourcing & Persistence Layer        │
                  │  (Event Store, Snapshots, Read Models, etc.)│
                  └──────────────────────┬──────────────────────┘
                                         │
                           ┌─────────────▼─────────────┐
                           │ Projections & Real-Time   │
                           │  (TimeSeries + SignalR)   │
                           └───────────────────────────┘
```

## Getting Started

### Prerequisites

- .NET SDK 8.0 (preview tolerated by the repo).
- PostgreSQL with TimescaleDB extension (optional, used for sensor streaming).
- SQL Server or SQLite (depending on selected persistence mode).

### Clone & Build

```bash
git clone <repo-url>
cd Quasar/src
dotnet build Quasar.sln
dotnet test Quasar.Tests/Quasar.Tests.csproj
```

### Run the Sample API

```bash
dotnet run --project Quasar.Samples.BasicApi
```

Navigate to:

- **Swagger UI:** `http://localhost:<port>/` (root serving swagger because web UI served from `/app`).  
- **Sample Web Console:** `http://localhost:<port>/app/index.html`

Use the seeded credentials to login via the console:

- Username: `swagger-demo`
- Password: `Passw0rd!`

## Core Packages

### Quasar.Core

Lightweight primitives shared across the framework.

- `IClock`, `SystemClock` – time abstraction for deterministic testing.
- `GuidIds` – helper for GUID-based identifiers.
- `Result` / `Result<T>` / `Error` – value types for expressing service outcomes.

### Quasar.Cqrs

Mediator-powered command/query dispatch.

- Marker interfaces: `ICommand<TResult>`, `IQuery<TResult>`.
- Handler interfaces and mediator pipeline behaviors (`ValidationBehavior`, `TransactionBehavior`, `AuthorizationBehavior`).
- Default `Mediator` implementation using DI.

### Quasar.Domain

Domain-driven building blocks.

- `AggregateRoot` – base class supporting event sourcing and domain events.
- `ValueObject` – structural equality for immutable value types.
- `IDomainEvent` – marker interface for domain events.

### Quasar.EventSourcing

Abstractions (`IEvent`, `EventEnvelope`, `IEventStore`, `ISnapshotStore`) and repository support.

Implementations provided:

- **In-memory store** for testing.
- **SQLite store** with `SqliteEventStore`, `SqliteSnapshotStore`, `SqliteCommandTransaction`.
- **SQL Server store** (via `SqlEventStore`, `SqlSnapshotStore`, `SqlCommandTransaction`).

### Quasar.Persistence

Read model abstractions (`IReadRepository<T>`, `IDocumentCollection<TDocument>`, `ITimeSeriesWriter/Reader`). EF Core-based repositories live in `Quasar.Persistence.Relational.EfCore`.

### Quasar.Security

Authorization infrastructure.

- `AclEntry`, `AclEffect`, `IAuthorizationService`, `ITokenService`.
- `AuthorizationBehavior` for mediator pipeline.

### Quasar.Logging

Serilog integration helpers via `UseQuasarSerilog` and `LoggingOptions` (console, file, Seq sinks).

### Quasar.RealTime

Bridges events to time-series persistence + real-time notifications.

- `RealTimeProjection<TEvent, TPayload>` persists incoming events via `ITimeSeriesWriter` and pushes payloads through an `IRealTimeNotifier` implementation.
- Delegate adapters simplify mapping from event types to time-series points or SignalR payloads.

`Quasar.RealTime.SignalR` adds strongly typed hubs, dispatchers, and DI helpers for SignalR-based broadcasting.

## Identity & ACL

`Quasar.Identity` + `Quasar.Identity.Persistence.Relational.EfCore` provide event-sourced users, roles, permissions and EF Core projections. The sample seeds a demo user/role granting:

- `counter.increment`
- `cart.add`
- `sensor.ingest`

Identity endpoints exposed in the sample API (`/auth/register`, `/auth/login`, `/auth/roles`, `/auth/acl`, etc.) demonstrate registration, JWT issuance with refresh tokens, role/permission management, and ACL evaluation inside the mediator pipeline.

## Time-Series & SignalR Streaming

`Quasar.Persistence.TimeSeries.Timescale` integrates with TimescaleDB:

- Creates the database (if missing) and ensures the hypertable schema.
- Implements `ITimeSeriesWriter` / `ITimeSeriesReader` for efficient ingestion and querying.

`Quasar.RealTime` ties events to SignalR:

- `SensorRealTimeProjection` in the sample transforms sensor events into Timescale points and broadcasts payloads via `SensorHub`.
- `SignalRServiceCollectionExtensions.AddSignalRNotifier` registers typed dispatchers + notifiers.

## Sample Web UI

Located under `Quasar.Samples.BasicApi/wwwroot/app`.

Features:

- Register & login with JWT tokens (credentials prefilled).
+- Counter and Cart demo endpoints.
− Sensor ingestion & history querying (requires TimescaleDB connection).
− Real-time feed of sensor readings via SignalR.
− Live logging pane for API responses.

## Extending Quasar

1. **Add a new aggregate**
   - Derive from `AggregateRoot`, define events + handlers.
   - Create command/query handlers wired via DI in `Program.cs`.
2. **Create projections**
   - Implement `IProjection<TEvent>` to translate events into read models.
   - Register as `services.AddScoped<object, YourProjection>();` to be picked up by the polling projector.
3. **Secure requests**
   - Implement `IAuthorizableRequest` on commands/queries.
   - Provide authorization logic through `IAuthorizationService` (EF Core-backed sample included).
4. **Add real-time broadcasts**
   - Map events to `TimeSeriesPoint` and payloads via adapters.
   - Create a typed `RealTimeProjection` and register it together with a SignalR hub.

## Testing

Run unit tests:

```bash
dotnet test Quasar.Tests/Quasar.Tests.csproj
```

Tests cover mediator behavior, in-memory event store, and utility types. Extend the suite to cover new aggregates or projections. For integration tests, you can spin up SQL/Timescale containers using Docker.

## License

This project is provided under the MIT License. See [LICENSE](LICENSE) for details.
