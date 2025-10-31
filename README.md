# Quasar Micro-Framework

Quasar is a modular .NET micro-framework for CQRS, event sourcing, identity, scheduling, logging, and real-time streaming. It bundles a mediator-centric command pipeline, pluggable event stores, EF Core read models, ACL/Identity, Quartz-based scheduling, and SignalR time-series streaming.

[![Build Status](https://github.com/fathorMB/Quasar/actions/workflows/publish-packages.yml/badge.svg)](https://github.com/fathorMB/Quasar/actions/workflows/publish-packages.yml)

## Package Badges

| Package | Status |
| --- | --- |
| Quasar.Core | [![Quasar.Core](https://img.shields.io/badge/GitHub%20Packages-Quasar.Core-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.core) |
| Quasar.Cqrs | [![Quasar.Cqrs](https://img.shields.io/badge/GitHub%20Packages-Quasar.Cqrs-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.cqrs) |
| Quasar.Domain | [![Quasar.Domain](https://img.shields.io/badge/GitHub%20Packages-Quasar.Domain-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.domain) |
| Quasar.EventSourcing.Abstractions | [![Quasar.EventSourcing.Abstractions](https://img.shields.io/badge/GitHub%20Packages-Quasar.EventSourcing.Abstractions-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.eventsourcing.abstractions) |
| Quasar.EventSourcing.InMemory | [![Quasar.EventSourcing.InMemory](https://img.shields.io/badge/GitHub%20Packages-Quasar.EventSourcing.InMemory-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.eventsourcing.inmemory) |
| Quasar.EventSourcing.Sqlite | [![Quasar.EventSourcing.Sqlite](https://img.shields.io/badge/GitHub%20Packages-Quasar.EventSourcing.Sqlite-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.eventsourcing.sqlite) |
| Quasar.EventSourcing.SqlServer | [![Quasar.EventSourcing.SqlServer](https://img.shields.io/badge/GitHub%20Packages-Quasar.EventSourcing.SqlServer-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.eventsourcing.sqlserver) |
| Quasar.Persistence.Abstractions | [![Quasar.Persistence.Abstractions](https://img.shields.io/badge/GitHub%20Packages-Quasar.Persistence.Abstractions-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.persistence.abstractions) |
| Quasar.Persistence.Relational.EfCore | [![Quasar.Persistence.Relational.EfCore](https://img.shields.io/badge/GitHub%20Packages-Quasar.Persistence.Relational.EfCore-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.persistence.relational.efcore) |
| Quasar.Persistence.TimeSeries.Timescale | [![Quasar.Persistence.TimeSeries.Timescale](https://img.shields.io/badge/GitHub%20Packages-Quasar.Persistence.TimeSeries.Timescale-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.persistence.timeseries.timescale) |
| Quasar.Projections.Abstractions | [![Quasar.Projections.Abstractions](https://img.shields.io/badge/GitHub%20Packages-Quasar.Projections.Abstractions-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.projections.abstractions) |
| Quasar.Projections.Sqlite | [![Quasar.Projections.Sqlite](https://img.shields.io/badge/GitHub%20Packages-Quasar.Projections.Sqlite-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.projections.sqlite) |
| Quasar.Projections.SqlServer | [![Quasar.Projections.SqlServer](https://img.shields.io/badge/GitHub%20Packages-Quasar.Projections.SqlServer-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.projections.sqlserver) |
| Quasar.RealTime | [![Quasar.RealTime](https://img.shields.io/badge/GitHub%20Packages-Quasar.RealTime-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.realtime) |
| Quasar.RealTime.SignalR | [![Quasar.RealTime.SignalR](https://img.shields.io/badge/GitHub%20Packages-Quasar.RealTime.SignalR-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.realtime.signalr) |
| Quasar.Scheduling.Quartz | [![Quasar.Scheduling.Quartz](https://img.shields.io/badge/GitHub%20Packages-Quasar.Scheduling.Quartz-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.scheduling.quartz) |
| Quasar.Identity | [![Quasar.Identity](https://img.shields.io/badge/GitHub%20Packages-Quasar.Identity-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.identity) |
| Quasar.Identity.Persistence.Relational.EfCore | [![Quasar.Identity.Persistence.Relational.EfCore](https://img.shields.io/badge/GitHub%20Packages-Quasar.Identity.Persistence.Relational.EfCore-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.identity.persistence.relational.efcore) |
| Quasar.Identity.Web | [![Quasar.Identity.Web](https://img.shields.io/badge/GitHub%20Packages-Quasar.Identity.Web-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.identity.web) |
| Quasar.Logging | [![Quasar.Logging](https://img.shields.io/badge/GitHub%20Packages-Quasar.Logging-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.logging) |
| Quasar.Security | [![Quasar.Security](https://img.shields.io/badge/GitHub%20Packages-Quasar.Security-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.security) |
| Quasar.Web | [![Quasar.Web](https://img.shields.io/badge/GitHub%20Packages-Quasar.Web-24292e?logo=github)](https://github.com/fathorMB?tab=packages&repo_name=Quasar&package_type=nuget&package_name=quasar.web) |

## Table of Contents

- [Package Badges](#package-badges)
- [Highlights](#highlights)
- [Architecture Overview](#architecture-overview)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Clone & Build](#clone--build)
  - [Run the Sample API](#run-the-sample-api)
- [Core Packages](#core-packages)
- [Practical Modelling Examples](#practical-modelling-examples)
- [Identity & ACL](#identity--acl)
- [Time-Series & SignalR Streaming](#time-series--signalr-streaming)
- [Quartz Scheduling](#quartz-scheduling)
- [Sample Web UI](#sample-web-ui)
- [Developer Templates](#developer-templates)
- [Extending Quasar](#extending-quasar)
- [Testing](#testing)
- [License](#license)

## Highlights

- **Mediator-centric CQRS** with pipeline behaviors for validation, transactions, and authorization.
- **Event-sourced aggregates** with pluggable stores (SQL Server, SQLite, in-memory).
- **Relational/document/time-series read models** wired with EF Core and TimescaleDB integrations.
- **Identity & ACL** with JWT issuance, refresh tokens, and granular role/permission management.
- **Serilog logging** helpers with console/file/Seq and in-memory buffering, ready for telemetry.
- **Quartz scheduling** with automatic schema provisioning and DI-based jobs.
- **Real-time streaming** via SignalR bridging event streams to clients.
- **End-to-end sample** showcasing counter/cart flows, sensor telemetry, identity, and real-time dashboards.

## Architecture Overview

```
+--------------------+    +--------------+
|  API Endpoints     | -> |   Mediator   | --+
+--------------------+    +--------------+   |
                                            |
                               +---------------------------+
                               |   Pipeline Behaviors      |
                               | (Validation, Transaction, |
                               |  Authorization, etc.)     |
                               +------------+--------------+
                                            |
                              +-------------v--------------+
                              |   Command / Query          |
                              |         Handlers           |
                              +-------------+--------------+
                                            |
+------------------------------v---------------------------+
|   Event Sourcing & Persistence Layer                     |
|   (Event Store, Snapshots, Read Models, ACL, Logging)    |
+------------------------------+---------------------------+
                               |
                    +----------v-----------+
                    | Projections &        |
                    | Real-Time Streaming  |
                    +----------------------+
```

## Getting Started

### Prerequisites

- .NET SDK 8.0 or newer (preview tolerated by the repo).
- SQL Server or SQLite (depending on the persistence mode).
- PostgreSQL with TimescaleDB (optional; required for the sensor telemetry demo).

### Clone & Build

```bash
git clone https://github.com/fathorMB/Quasar
cd Quasar/src
dotnet build Quasar.sln
dotnet test Quasar.Tests/Quasar.Tests.csproj
```

### Run the Sample API

```bash
dotnet run --project Quasar.Samples.BasicApi
```

Browse to:

- **Swagger UI:** `http://localhost:<port>/swagger`
- **Sample Web Console:** `http://localhost:<port>/app/index.html`

Seeded credentials:

- Username: `swagger-demo`
- Password: `Passw0rd!`

## Core Packages

Each subdirectory under `src/` represents a NuGet package published via GitHub Packages:

- **Quasar.Core** � core primitives (`IClock`, strongly-typed IDs, `Result` types).
- **Quasar.Cqrs** � mediator, command/query abstractions, pipeline behaviors.
- **Quasar.Domain** � base `AggregateRoot`, event sourcing helpers, value objects.
- **Quasar.EventSourcing.\*** - event store abstractions plus in-memory/SQLite/SQL Server implementations.
- **Quasar.Persistence.\*** - read-model abstractions, EF Core integration, TimescaleDB time-series support.
- **Quasar.Identity.\*** - event-sourced identity, persistence, Web APIs.
- **Quasar.Logging** � Serilog setup helper.
- **Quasar.RealTime** � SignalR & Timescale integration.
- **Quasar.Scheduling.Quartz** � Quartz hosting, schema bootstrapper, REST endpoints.

## Practical Modelling Examples

### Aggregate & Commands

```csharp
public sealed record CartCreated(Guid CartId) : IDomainEvent;
public sealed record CartItemAdded(Guid CartId, Guid ProductId, int Quantity) : IDomainEvent;

public sealed class CartAggregate : AggregateRoot
{
    private readonly List<CartLine> _lines = new();

    private CartAggregate() { }

    public static CartAggregate Create(Guid cartId)
    {
        var cart = new CartAggregate();
        cart.ApplyChange(new CartCreated(cartId));
        return cart;
    }

    public void AddItem(Guid productId, int quantity)
    {
        if (quantity <= 0) throw new InvalidOperationException("Quantity must be positive.");
        ApplyChange(new CartItemAdded(Id, productId, quantity));
    }

    protected override void When(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case CartCreated created:
                Id = created.CartId;
                break;
            case CartItemAdded added:
                _lines.Add(new CartLine(added.ProductId, added.Quantity));
                break;
        }
    }

    private sealed record CartLine(Guid ProductId, int Quantity);
}

public sealed record AddCartItem(Guid CartId, Guid ProductId, int Quantity) : ICommand<int>;

public sealed class AddCartItemHandler : ICommandHandler<AddCartItem, int>
{
    private readonly IEventSourcedRepository<CartAggregate> _repository;

    public AddCartItemHandler(IEventSourcedRepository<CartAggregate> repository)
        => _repository = repository;

    public async Task<int> Handle(AddCartItem request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetAsync(request.CartId, cancellationToken);
        if (cart.Id == Guid.Empty)
        {
            cart = CartAggregate.Create(request.CartId);
        }

        cart.AddItem(request.ProductId, request.Quantity);
        await _repository.SaveAsync(cart, cancellationToken);

        return cart.Version;
    }
}

public sealed class AddCartItemValidator : AbstractValidator<AddCartItem>
{
    public AddCartItemValidator()
    {
        RuleFor(c => c.CartId).NotEmpty();
        RuleFor(c => c.ProductId).NotEmpty();
        RuleFor(c => c.Quantity).GreaterThan(0);
    }
}
```

### Projection & Read Model

Read models are now schema-first: register the store metadata and let Quasar create the relational schema on application start.

1. Create a marker implementing `IReadModelStoreMarker` and a definition derived from `ReadModelDefinition<TStore>` where you configure the EF Core entities.
2. Register the definition (`services.AddReadModelDefinition<MyStoreDefinition>()`) and choose a provider with `UseEfCoreSqlServerReadModels<TStore>()` or `UseEfCoreSqliteReadModels<TStore>()`.
3. Before running custom seeders call `await host.InitializeReadModelsAsync();` so the framework can create any missing tables/columns for each registered store.
4. Implement projections by injecting `ReadModelContext<TStore>` or `IReadRepository<T>` and mutating the read models when events are replayed.

The framework inspects the assembled EF model at runtime and issues the required DDL (creates tables everywhere, adds new nullable columns on SQL Server). The sample therefore no longer ships EF Core migrations.


```csharp
public sealed class CartReadModel
{
    public Guid Id { get; set; }
    public List<CartLineDto> Lines { get; } = new();
}

public sealed record CartLineDto(Guid ProductId, int Quantity);

public sealed class CartProjection : IProjection<CartItemAdded>
{
    private readonly IReadRepository<CartReadModel> _repository;

    public CartProjection(IReadRepository<CartReadModel> repository)
        => _repository = repository;

    public async Task HandleAsync(CartItemAdded domainEvent, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByIdAsync(domainEvent.CartId, cancellationToken);
        if (cart is null)
        {
            cart = new CartReadModel { Id = domainEvent.CartId };
            cart.Lines.Add(new CartLineDto(domainEvent.ProductId, domainEvent.Quantity));
            await _repository.AddAsync(cart, cancellationToken);
            return;
        }

        cart.Lines.Add(new CartLineDto(domainEvent.ProductId, domainEvent.Quantity));
        await _repository.UpdateAsync(cart, cancellationToken);
    }
}
```

Register handlers, validators, and projections in DI:

```csharp
services.AddScoped<ICommandHandler<AddCartItem, int>, AddCartItemHandler>();
services.AddScoped<IValidator<AddCartItem>, AddCartItemValidator>();
services.AddScoped<object, CartProjection>();
```

### Real-Time & Scheduling

```csharp
public sealed class RebuildProjectionsJob : IQuasarJob
{
    private readonly IMediator _mediator;

    public RebuildProjectionsJob(IMediator mediator) => _mediator = mediator;

    public Task ExecuteAsync(QuasarJobContext context, CancellationToken cancellationToken)
        => _mediator.Send(new TriggerRebuildCommand(), cancellationToken);
}

services.AddQuartzScheduler(options =>
{
    options.SchedulerName = "Maintenance";
    options.Configure = builder =>
    {
        builder.ScheduleQuasarJob<RebuildProjectionsJob>(
            job => job.WithIdentity("rebuild", "maintenance"),
            trigger => trigger
                .WithIdentity("rebuild-nightly", "maintenance")
                .WithCronSchedule("0 0 3 * * ?"));
    };
});
```

## Identity & ACL

`Quasar.Identity` plus `Quasar.Identity.Persistence.Relational.EfCore` provide event-sourced users, roles, and permissions. The sample seeds a demo user assigned to a role granting `counter.increment`, `cart.add`, and `sensor.ingest` permissions. The `/auth` endpoints demonstrate registration, login, JWT issuance, and ACL evaluation.

## Time-Series & SignalR Streaming

`Quasar.Persistence.TimeSeries.Timescale` configures TimescaleDB. `Quasar.RealTime` persists sensor events to hypertables and broadcasts payloads through a strongly-typed SignalR hub.

## Quartz Scheduling

The Quartz integration auto-provisions the job-store schema (SQL Server/SQLite), registers a DI-friendly job factory, and exposes management endpoints:

- `GET /quartz/jobs`
- `POST /quartz/jobs/{group}/{name}/trigger`
- `POST /quartz/jobs/{group}/{name}/pause`
- `POST /quartz/jobs/{group}/{name}/resume`

## Saga Orchestration Sample

The `Quasar.Samples.BasicApi` project now ships with a checkout saga that demonstrates the framework-level orchestration primitives. The saga coordinates three commands and persists its state via the default in-memory saga repository.

Saga state persistence is now provider-agnostic: configure `AddQuasarSagas` with `builder => builder.UseSqlServerSagaStore(...)` or `UseSqliteSagaStore(...)` to reuse the same EF Core connector powering your identity/read model stores.
1. Start a checkout and capture the saga id (omit `checkoutId` to auto-generate):
   ```bash
   curl -X POST http://localhost:5236/checkout/start \
        -H "Content-Type: application/json" \
        -d '{"totalAmount": 125.50}'
   ```
2. Confirm payment using the identifier returned from step 1:
   ```bash
   curl -X POST http://localhost:5236/checkout/{checkoutId}/payment \
        -H "Content-Type: application/json" \
        -d '{"paymentReference":"PAY-12345"}'
   ```
3. Request fulfillment (optionally include a tracking number):
   ```bash
   curl -X POST http://localhost:5236/checkout/{checkoutId}/fulfillment \
        -H "Content-Type: application/json" \
        -d '{"trackingNumber":"TRACK-7890"}'
   ```
4. Inspect the live saga state:
   ```bash
   curl http://localhost:5236/checkout/{checkoutId}
   ```

All endpoints accept the optional `X-Subject` header to simulate authenticated subjects; when omitted, the seeded demo user is used.

## Sample Web UI

`Quasar.Samples.BasicApi/wwwroot/app` provides a control panel to:

- Authenticate using seeded credentials.
- Increment counters and manage carts.
- Ingest/query sensor readings (Timescale optional).
- Watch real-time SignalR streams.
- Inspect live logs via `/debug/logs/recent` polling.

## Developer Templates

A starter `dotnet new` template lives under `templates/quasar-service`. It scaffolds a minimal API already wired up to Quasar packages.

```bash
# install locally from the repo root
dotnet new --install ./templates/quasar-service

# scaffold a new API referencing the published packages
dotnet new quasar-service --name My.Quasar.Service --quasarVersion 0.1.0-preview.1
```

Template tips:

- `--quasarVersion` defaults to `*`, so the latest package on your configured feed is used if the parameter is omitted.
- Run the publish workflow after bumping `version.json` if you want the template to target a freshly released version.
- Remove the local template with `dotnet new --uninstall ./templates/quasar-service` when you are done experimenting.

## Extending Quasar

1. **Add aggregates** � derive from `AggregateRoot`, define domain events, and wire command/query handlers.
2. **Create projections** � implement `IProjection<TEvent>` and register it so the polling projector picks it up.
3. **Secure requests** � implement `IAuthorizableRequest` and evaluate permissions via `IAuthorizationService`.
4. **Broadcast** � map events to `TimeSeriesPoint` and SignalR payloads using the real-time adapters.

## Testing

```bash
dotnet test Quasar.Tests/Quasar.Tests.csproj
```

The test suite covers mediator behavior, event store logic, scheduling infrastructure, and more. Extend it as new modules ship.

## License

This project is provided under the MIT License. See [LICENSE](LICENSE) for details.


