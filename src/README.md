# Quasar Micro-Framework

Quasar is a .NET 8 micro-framework for building CQRS and event-sourced APIs. It combines a mediator, pluggable event storage, rich read-model projections, identity & ACL, logging, and optional real-time streaming.

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
- [Quartz Scheduling](#quartz-scheduling)
- [Sample Web UI](#sample-web-ui)
- [Extending Quasar](#extending-quasar)
- [Testing](#testing)
- [License](#license)

## Highlights

- **Mediator-centric CQRS** with pipeline behaviors for validation, transactions, and authorization.
- **Event-sourced aggregates** with pluggable stores (SQL Server, SQLite, in-memory).
- **Relational read models** powered by EF Core plus optional document & time-series abstractions.
- **Identity & ACL** with JWT issuance, refresh tokens, and dynamic role/permission management.
- **Logging helpers** via Serilog (console, file, Seq, in-memory buffer).
- **Real-time streaming** via SignalR with TimescaleDB-backed time-series persistence.
- **End-to-end sample** demonstrating counter/cart flows, sensor telemetry, and real-time dashboards.

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

- .NET SDK 8.0 (preview tolerated by the repo).
- SQL Server or SQLite (depending on the selected persistence mode).
- PostgreSQL with TimescaleDB (optional; only required for sensor telemetry demo).

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

Browse to:

- **Swagger UI:** `http://localhost:<port>/swagger`
- **Sample Web Console:** `http://localhost:<port>/app/index.html`

Seeded credentials for the console:

- Username: `swagger-demo`
- Password: `Passw0rd!`

## Core Packages

### Quasar.Core

Shared primitives such as `IClock`, strongly-typed identifiers, and `Result`/`Error` value types.

### Quasar.Cqrs

Mediator, command/query abstractions, and pipeline behaviors (validation, transactions, authorization).

### Quasar.Domain

Event-sourced aggregate support (`AggregateRoot`), value objects, and domain event plumbing.

### Quasar.EventSourcing

Abstractions plus implementations for in-memory, SQLite, and SQL Server event stores and snapshots.

### Quasar.Persistence

Read model abstractions (`IReadRepository<T>`, `IDocumentCollection<TDocument>`, `ITimeSeriesWriter/Reader`) with EF Core integrations.

### Quasar.Security

Identity & ACL primitives, authorization behaviors, and JWT token services.

### Quasar.Logging

Serilog configuration extensions (`UseQuasarSerilog`) with console/file/Seq/in-memory buffering options.

### Quasar.RealTime

Adapters for bridging events to time-series persistence and SignalR hubs.

## Practical Modelling Examples

The snippets below illustrate how you can compose the building blocks provided by Quasar when modelling your own bounded contexts.

### Aggregate & Commands

```csharp
// Domain event definitions
public sealed record CartCreated(Guid CartId) : IDomainEvent;
public sealed record CartItemAdded(Guid CartId, Guid ProductId, int Quantity) : IDomainEvent;

// Aggregate rooted in Quasar.Domain.AggregateRoot
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

// Command definition
public sealed record AddCartItem(Guid CartId, Guid ProductId, int Quantity) : ICommand<int>;

// Command handler using the Quasar.Cqrs infrastructure
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

// FluentValidation-based validator wired into the pipeline
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

### Projection & Read Model Wiring

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

Register projections (and validators/handlers) in `Program.cs` so the polling projector picks them up automatically:

```csharp
services.AddScoped<ICommandHandler<AddCartItem, int>, AddCartItemHandler>();
services.AddScoped<IValidator<AddCartItem>, AddCartItemValidator>();
services.AddScoped<object, CartProjection>(); // discovered by AddPollingProjector
```

### Real-Time & Scheduling

```csharp
// Job implementing Quasar's IQuasarJob abstraction
public sealed class RebuildProjectionsJob : IQuasarJob
{
    private readonly IMediator _mediator;

    public RebuildProjectionsJob(IMediator mediator) => _mediator = mediator;

    public Task ExecuteAsync(QuasarJobContext context, CancellationToken cancellationToken)
        => _mediator.Send(new TriggerRebuildCommand(), cancellationToken);
}

// Scheduler registration
services.AddQuartzScheduler(options =>
{
    options.SchedulerName = "Maintenance";
    options.Configure = builder =>
    {
        builder.ScheduleQuasarJob<RebuildProjectionsJob>(
            job => job.WithIdentity("rebuild", "maintenance"),
            trigger => trigger
                .WithIdentity("rebuild-nightly", "maintenance")
                .WithCronSchedule("0 0 3 * * ?")); // 3AM daily
    };
});
```

## Identity & ACL

`Quasar.Identity` and `Quasar.Identity.Persistence.Relational.EfCore` provide event-sourced users, roles, and permissions. The sample seeds:

- Role granting `counter.increment`, `cart.add`, and `sensor.ingest` permissions.
- Demo user (`swagger-demo`) assigned to the role.

Endpoints under `/auth` demonstrate registration, login, role administration, and ACL checks.

## Time-Series & SignalR Streaming

`Quasar.Persistence.TimeSeries.Timescale` configures TimescaleDB and exposes writers/readers.
`Quasar.RealTime` projects sensor readings into Timescale and broadcasts them via a typed SignalR hub.

## Quartz Scheduling

Quartz integration is built into the framework:

```csharp
services.AddQuartzScheduler(options =>
{
    options.SchedulerName = "SampleScheduler";
    options.Configure = builder =>
    {
        builder.ScheduleQuasarJob<IncrementCounterJob>(
            job => job.WithIdentity("counter", "demo"),
            trigger => trigger
                .WithIdentity("counter-trigger", "demo")
                .WithSimpleSchedule(s => s.WithInterval(TimeSpan.FromMinutes(1)).RepeatForever())
                .StartNow());
    };
});

app.MapQuartzEndpoints();
```

When configured with SQL Server or SQLite, Quasar automatically creates the Quartz schema during startup and exposes management endpoints:

- `GET /quartz/jobs`
- `POST /quartz/jobs/{group}/{name}/trigger`
- `POST /quartz/jobs/{group}/{name}/pause`
- `POST /quartz/jobs/{group}/{name}/resume`

## Sample Web UI

Located in `Quasar.Samples.BasicApi/wwwroot/app`, the console allows you to:

- Register and log in (with pre-filled demo credentials).
- Increment the counter and manage cart items.
- Ingest and query sensor readings.
- View real-time sensor broadcasts over SignalR.
- Inspect server-side logs (including background job output) via a live polling pane.

## Extending Quasar

1. **Add a new aggregate** – derive from `AggregateRoot`, define events and handlers, wire commands/queries in DI.
2. **Create projections** – implement `IProjection<TEvent>` and register it to participate in polling projectors.
3. **Secure requests** – implement `IAuthorizableRequest`, add ACL checks via `IAuthorizationService`.
4. **Broadcast in real time** – map events to `TimeSeriesPoint` and typed hub payloads using real-time adapters.

## Testing

Run unit tests with:

```bash
dotnet test Quasar.Tests/Quasar.Tests.csproj
```

The suite covers mediator behaviors, event store logic, and scheduling infrastructure. Extend it as you add domain features.

## License

Released under the MIT License. See [LICENSE](LICENSE) for details.
