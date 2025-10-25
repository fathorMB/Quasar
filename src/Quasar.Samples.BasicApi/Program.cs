using System;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Quasar.Cqrs;
using Quasar.EventSourcing.Abstractions;
using Quasar.EventSourcing.InMemory;
using Quasar.EventSourcing.SqlServer;
using Quasar.EventSourcing.Sqlite;
using Quasar.Samples.BasicApi;
using Quasar.Web;
using Quasar.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using Quasar.Projections.Abstractions;
using Quasar.Identity.Web;
using Quasar.Identity.Persistence.Relational.EfCore;
using Quasar.Identity.Persistence.Relational.EfCore.Seeding;
using Quasar.Logging;
using Quasar.Seeding;
using Serilog.Events;
using System.IO;
using System.Linq;
using Quasar.Core;
using Quasar.Samples.BasicApi.Swagger;
using Quasar.Samples.BasicApi.RealTime;
using Quasar.RealTime;
using Quasar.RealTime.SignalR;
using Quasar.Scheduling.Quartz;
using Quartz;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddSingleton<InMemoryLogBuffer>();

builder.Host.UseQuasarSerilog(configuration, "Logging", options =>
{
    options.UseConsole = true;
    options.UseFile = true;
    options.FilePath = Path.Combine(AppContext.BaseDirectory, "logs", "quasar.log");
    options.InMemoryBufferCapacity = 1024;
    options.LevelOverrides["Microsoft"] = LogEventLevel.Warning;
    options.LevelOverrides["Microsoft.EntityFrameworkCore"] = LogEventLevel.Warning;
    options.LevelOverrides["Quasar"] = LogEventLevel.Debug;
});

// CQRS + ES wiring
services.AddQuasarMediator();
services.AddQuasarEventSourcingCore();

// Event serializer mapping
IEventTypeMap typeMap = new DictionaryEventTypeMap(new[]
{
    ("counter.incremented", typeof(CounterIncremented)),
    ("cart.item_added", typeof(CartItemAdded)),
    ("cart.item_removed", typeof(CartItemRemoved)),
    ("identity.user_registered", typeof(Quasar.Identity.UserRegistered)),
    ("identity.user_password_set", typeof(Quasar.Identity.UserPasswordSet)),
    ("identity.user_role_assigned", typeof(Quasar.Identity.UserRoleAssigned)),
    ("identity.user_role_revoked", typeof(Quasar.Identity.UserRoleRevoked)),
    ("identity.role_created", typeof(Quasar.Identity.RoleCreated)),
    ("identity.role_renamed", typeof(Quasar.Identity.RoleRenamed)),
    ("identity.role_permission_granted", typeof(Quasar.Identity.RolePermissionGranted)),
    ("identity.role_permission_revoked", typeof(Quasar.Identity.RolePermissionRevoked)),
    ("sensor.reading_recorded", typeof(SensorReadingRecorded))
});
services.AddQuasarEventSerializer(typeMap);

services.AddQuasarSignalR();

// Swagger
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(options =>
{
    options.SchemaFilter<IdentitySchemaExamples>();
});

// Identity core services (must be registered before Build)
services.AddQuasarIdentity(o =>
{
    o.Key = "sample-super-secret-key-should-be-long";
    o.Issuer = "quasar"; o.Audience = "quasar";
});
services.AddIdentityDataSeeding(options =>
{
    var sampleSet = new IdentitySeedSet { Name = "Sample ACL" };

    var demoRole = new IdentityRoleSeed
    {
        Id = SampleConfig.DemoRoleId,
        Name = SampleConfig.DemoRoleName
    };
    demoRole.Permissions.Add(SampleConfig.CartAddPermission);
    demoRole.Permissions.Add("counter.increment");
    demoRole.Permissions.Add("sensor.ingest");
    sampleSet.Roles.Add(demoRole);

    var demoUser = new IdentityUserSeed
    {
        Id = SampleConfig.DemoUserId,
        Username = "swagger-demo",
        Email = "swagger-demo@example.com",
        Password = "Passw0rd!"
    };
    demoUser.Roles.Add(SampleConfig.DemoRoleId);
    sampleSet.Users.Add(demoUser);

    options.Sets.Add(sampleSet);
});

var timescaleSection = configuration.GetSection("Timescale");
var timescaleConnString = timescaleSection.GetValue<string>("ConnectionString")
    ?? configuration.GetConnectionString("QuasarTimescale")
    ?? "Host=localhost;Port=5432;Username=postgres;Password=mypassword;Database=quasar";
var timescaleDatabase = timescaleSection.GetValue<string>("Database");
services.UseTimescaleTimeSeries(options =>
{
    options.ConnectionString = timescaleConnString;
    options.Database = timescaleDatabase;
    options.MetricsTable = SensorConstants.MetricName;
    options.Schema = "public";
    options.WriteBatchSize = 500;
});

services.AddSingleton<SensorTimeSeriesAdapter>();
services.AddSingleton<SensorPayloadAdapter>();
services.AddSignalRNotifier<SensorHub, ISensorClient, SensorReadingPayload, SensorDispatcher>();
services.AddTransient<IncrementCounterJob>();

var storeMode = configuration["Quasar:Store"]?.ToLowerInvariant() ?? "inmemory";
string? quartzConnectionString = null;
string? quartzProvider = null;
string? quartzDelegateType = null;
switch (storeMode)
{
    case "sqlserver":
        var sqlConnString = configuration.GetConnectionString("QuasarSqlServer")
            ?? throw new InvalidOperationException("Connection string 'QuasarSqlServer' is required for SQL Server store.");
        var sqlOptions = new SqlEventStoreOptions
        {
            ConnectionFactory = () => new SqlConnection(sqlConnString)
        };
        await SqlEventStoreInitializer.EnsureSchemaAsync(sqlOptions);
        services.UseSqlServerEventStore(sqlOptions);
        services.UseSqlServerCommandTransaction();
        // read models + projections
        services.UseEfCoreSqlServerReadModels<SampleReadModelContext>(sqlConnString);
        services.UseSqlServerProjectionCheckpoints(() => new SqlConnection(sqlConnString));
        services.AddScoped<object, CounterProjection>();
        services.AddScoped<object, ShoppingCartProjection>();
        services.AddScoped<object, Quasar.Identity.Persistence.Relational.EfCore.IdentityProjections>();
        services.AddScoped<object, SensorRealTimeProjection>();
        services.AddPollingProjector("MainProjector", new[] { SampleConfig.CounterStreamId, SampleConfig.CartStreamId, SampleConfig.SensorStreamId }, TimeSpan.FromMilliseconds(500));
        // Identity read models
        services.UseEfCoreSqlServerReadModels<IdentityReadModelContext>(sqlConnString, registerRepositories: false);
        quartzConnectionString = sqlConnString;
        quartzProvider = "SqlServer";
        quartzDelegateType = "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz";
        break;
    case "sqlite":
        var sqliteConnString = configuration.GetConnectionString("QuasarSqlite") ?? "Data Source=quasar.db";
        var sqliteOptions = new SqliteEventStoreOptions
        {
            ConnectionFactory = () => new SqliteConnection(sqliteConnString)
        };
        await SqliteEventStoreInitializer.EnsureSchemaAsync(sqliteOptions);
        services.UseSqliteEventStore(sqliteOptions);
        services.UseSqliteCommandTransaction();
        // read models + projections
        services.UseEfCoreSqliteReadModels<SampleReadModelContext>(sqliteConnString);
        services.UseSqliteProjectionCheckpoints(() => new SqliteConnection(sqliteConnString));
        services.AddScoped<object, CounterProjection>();
        services.AddScoped<object, ShoppingCartProjection>();
        services.AddScoped<object, Quasar.Identity.Persistence.Relational.EfCore.IdentityProjections>();
        services.AddScoped<object, SensorRealTimeProjection>();
        services.AddPollingProjector("MainProjector", new[] { SampleConfig.CounterStreamId, SampleConfig.CartStreamId, SampleConfig.SensorStreamId }, TimeSpan.FromMilliseconds(500));
        // Identity read models
        services.UseEfCoreSqliteReadModels<IdentityReadModelContext>(sqliteConnString, registerRepositories: false);
        quartzConnectionString = sqliteConnString;
        quartzProvider = "SQLite-Microsoft";
        quartzDelegateType = "Quartz.Impl.AdoJobStore.SQLiteDelegate, Quartz";
        break;
    default:
        services.UseInMemoryEventStore();
        services.AddScoped<ICommandTransaction, NoopCommandTransaction>();
        // use sqlite for read models while using in-memory event store for demo
        var demoSqlite = configuration.GetConnectionString("QuasarSqlite") ?? "Data Source=quasar.db";
        services.UseEfCoreSqliteReadModels<SampleReadModelContext>(demoSqlite);
        services.UseSqliteProjectionCheckpoints(() => new SqliteConnection(demoSqlite));
        services.AddScoped<object, CounterProjection>();
        services.AddScoped<object, ShoppingCartProjection>();
        services.AddScoped<object, Quasar.Identity.Persistence.Relational.EfCore.IdentityProjections>();
        services.AddScoped<object, SensorRealTimeProjection>();
        services.AddPollingProjector("MainProjector", new[] { SampleConfig.CounterStreamId, SampleConfig.CartStreamId, SampleConfig.SensorStreamId }, TimeSpan.FromMilliseconds(500));
        // Identity read models
        services.UseEfCoreSqliteReadModels<IdentityReadModelContext>(demoSqlite, registerRepositories: false);
        quartzConnectionString = demoSqlite;
        quartzProvider = "SQLite-Microsoft";
        quartzDelegateType = "Quartz.Impl.AdoJobStore.SQLiteDelegate, Quartz";
        break;
}

services.AddQuartzScheduler(options =>
{
    options.SchedulerName = "SampleScheduler";
    if (!string.IsNullOrWhiteSpace(quartzConnectionString)
        && !string.IsNullOrWhiteSpace(quartzProvider)
        && !string.IsNullOrWhiteSpace(quartzDelegateType))
    {
        options.FactoryProperties["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
        options.FactoryProperties["quartz.jobStore.useProperties"] = "true";
        options.FactoryProperties["quartz.jobStore.dataSource"] = "default";
        options.FactoryProperties["quartz.jobStore.tablePrefix"] = "QRTZ_";
        options.FactoryProperties["quartz.jobStore.clustered"] = "false";
        options.FactoryProperties["quartz.jobStore.driverDelegateType"] = quartzDelegateType;
        options.FactoryProperties["quartz.dataSource.default.provider"] = quartzProvider;
        options.FactoryProperties["quartz.dataSource.default.connectionString"] = quartzConnectionString;
        options.FactoryProperties["quartz.serializer.type"] = "Quartz.Simpl.BinaryObjectSerializer, Quartz";
    }

    options.Configure = builder =>
    {
        builder.ScheduleQuasarJob<IncrementCounterJob>(
            job => job.WithIdentity("counter", "demo"),
            trigger => trigger
                .WithIdentity("counter-trigger", "demo")
                .WithSimpleSchedule(s => s.WithInterval(TimeSpan.FromSeconds(1)).RepeatForever())
                .StartNow());
    };
});

// Handlers and validators
services.AddScoped<ICommandHandler<IncrementCounterCommand, Result<int>>, IncrementCounterHandler>();
services.AddScoped<IQueryHandler<GetCounterQuery, int>, GetCounterHandler>();
services.AddScoped<IValidator<IncrementCounterCommand>, IncrementCounterValidator>();
services.AddScoped<ICommandHandler<AddCartItemCommand, Result<int>>, AddCartItemHandler>();
services.AddScoped<IQueryHandler<GetCartQuery, CartReadModel>, GetCartHandler>();
services.AddScoped<IValidator<AddCartItemCommand>, AddCartItemValidator>();
services.AddScoped<ICommandHandler<IngestSensorReadingCommand, Result<bool>>, IngestSensorReadingHandler>();
services.AddScoped<IValidator<IngestSensorReadingCommand>, SensorIngestValidator>();
services.AddScoped<IQueryHandler<SensorReadingsQuery, IReadOnlyList<TimeSeriesPoint>>, SensorReadingsHandler>();

var app = builder.Build();

// Apply EF Core migrations for read models
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetService<SampleReadModelContext>();
    if (db is not null) await db.Database.MigrateAsync();
    var idDb = scope.ServiceProvider.GetService<IdentityReadModelContext>();
    if (idDb is not null) await idDb.Database.MigrateAsync();
}

await app.SeedDataAsync();

// Identity endpoints
app.MapQuasarIdentityEndpoints();

app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Quasar Sample API v1");
    options.RoutePrefix = string.Empty; // expose at root
});

app.MapQuartzEndpoints();

app.MapRealTimeHub<SensorHub>("/hubs/sensors");

// Endpoints
app.MapPost("/counter/increment", async (IMediator mediator, int amount, HttpRequest req) =>
{
    if (amount <= 0) return Results.BadRequest("amount is required");
    var subject = req.Headers.TryGetValue("X-Subject", out var s) && Guid.TryParse(s, out var sid) ? sid : SampleConfig.DemoUserId;
    var result = await mediator.Send(new IncrementCounterCommand(subject, amount));
    return result.IsSuccess ? Results.Ok(new { count = result.Value }) : Results.BadRequest(result.Error);
}).WithName("IncrementCounter")
  .WithTags("Counter");

app.MapGet("/counter", async (IReadRepository<CounterReadModel> repo) =>
{
    var row = await repo.GetByIdAsync(SampleConfig.CounterStreamId);
    var count = row?.Count ?? 0;
    return Results.Ok(new { count });
}).WithName("GetCounter")
  .WithTags("Counter");

app.MapPost("/cart/add", async (IMediator mediator, Guid productId, int quantity, HttpRequest req) =>
{
    if (productId == Guid.Empty || quantity <= 0) return Results.BadRequest("productId and quantity are required");
    var subject = req.Headers.TryGetValue("X-Subject", out var s) && Guid.TryParse(s, out var sid) ? sid : SampleConfig.DemoUserId;
    var result = await mediator.Send(new AddCartItemCommand(subject, productId, quantity));
    return result.IsSuccess ? Results.Ok(new { totalItems = result.Value }) : Results.BadRequest(result.Error);
}).WithName("AddCartItem").WithTags("Cart");

app.MapGet("/cart", async (IMediator mediator) =>
{
    var cart = await mediator.Send(new GetCartQuery());
    return Results.Ok(cart);
}).WithName("GetCart").WithTags("Cart");

app.MapPost("/sensors/ingest", async (IMediator mediator, SensorIngestRequest request, HttpRequest httpRequest) =>
{
    if (request is null) return Results.BadRequest("Payload required");
    var subject = httpRequest.Headers.TryGetValue("X-Subject", out var s) && Guid.TryParse(s, out var sid) ? sid : SampleConfig.DemoUserId;
    var timestamp = request.TimestampUtc ?? DateTime.UtcNow;
    var result = await mediator.Send(new IngestSensorReadingCommand(subject, request.DeviceId, request.SensorType, request.Value, timestamp));
    return result.IsSuccess ? Results.Accepted($"/sensors/{request.DeviceId}/readings") : Results.BadRequest(result.Error);
}).WithName("IngestSensorReading").WithTags("Sensors");

app.MapGet("/sensors/{deviceId:guid}/readings", async (IMediator mediator, Guid deviceId, DateTime? fromUtc, DateTime? toUtc) =>
{
    if (deviceId == Guid.Empty) return Results.BadRequest("deviceId is required");
    var to = toUtc ?? DateTime.UtcNow;
    var from = fromUtc ?? to.AddMinutes(-30);
    if (from > to) return Results.BadRequest("fromUtc must be <= toUtc");

    var points = await mediator.Send(new SensorReadingsQuery(deviceId, from, to));
    var payload = points.Select(p => new SensorReadingResponse(
        p.Timestamp,
        p.Fields.TryGetValue("value", out var value) ? value : double.NaN,
        p.Tags.TryGetValue("sensorType", out var sensorType) ? sensorType : string.Empty)).ToArray();

    return Results.Ok(payload);
}).WithName("GetSensorReadings").WithTags("Sensors");

// Debug endpoints
app.MapGet("/debug/logs/recent", (InMemoryLogBuffer buffer, long? since) =>
{
    var entries = buffer.GetEntries(since);
    return Results.Ok(entries);
}).WithName("DebugRecentLogs").WithTags("Debug");

app.MapGet("/debug/carts", async (IReadRepository<CartReadModel> repo) =>
{
    var items = await repo.ListAsync();
    return Results.Ok(items);
}).WithName("DebugListCarts").WithTags("Debug");

app.MapGet("/debug/cart-products", async (IReadRepository<CartProductLine> repo) =>
{
    var items = await repo.ListAsync();
    return Results.Ok(items);
}).WithName("DebugListCartProducts").WithTags("Debug");

app.MapGet("/debug/checkpoints", async (ICheckpointStore cps) =>
{
    var counterKey = $"MainProjector:{SampleConfig.CounterStreamId}";
    var cartKey = $"MainProjector:{SampleConfig.CartStreamId}";
    var sensorKey = $"MainProjector:{SampleConfig.SensorStreamId}";
    var counter = await cps.GetCheckpointAsync(counterKey);
    var cart = await cps.GetCheckpointAsync(cartKey);
    var sensor = await cps.GetCheckpointAsync(sensorKey);
    return Results.Ok(new { counterKey, counter, cartKey, cart, sensorKey, sensor });
}).WithName("DebugCheckpoints").WithTags("Debug");

app.MapPost("/debug/rebuild/cart", async (IServiceProvider sp) =>
{
    using var scope = sp.CreateScope();
    var store = scope.ServiceProvider.GetRequiredService<IEventStore>();
    var cps = scope.ServiceProvider.GetRequiredService<ICheckpointStore>();
    var events = await store.ReadStreamAsync(SampleConfig.CartStreamId, 0);

    // manual dispatch using the same logic as the projector
    foreach (var env in events)
    {
        var projections = scope.ServiceProvider.GetServices<object>();
        var evtType = env.Event.GetType();
        foreach (var proj in projections)
        {
            var pt = proj.GetType();
            foreach (var iface in pt.GetInterfaces())
            {
                if (!iface.IsGenericType) continue;
                if (iface.GetGenericTypeDefinition() != typeof(IProjection<>)) continue;
                var arg = iface.GetGenericArguments()[0];
                if (!arg.IsAssignableFrom(evtType)) continue;
                var method = pt.GetMethods()
                    .Where(m => m.Name == "HandleAsync")
                    .FirstOrDefault(m =>
                    {
                        var ps = m.GetParameters();
                        return ps.Length == 2 && ps[0].ParameterType.IsAssignableFrom(evtType) && ps[1].ParameterType == typeof(CancellationToken);
                    });
                if (method is null) continue;
                await (Task)method.Invoke(proj, new object?[] { env.Event, CancellationToken.None })!;
                break;
            }
        }
        await cps.SaveCheckpointAsync($"MainProjector:{SampleConfig.CartStreamId}", env.Version);
    }
    return Results.Ok(new { rebuilt = events.Count });
}).WithName("DebugRebuildCart").WithTags("Debug");

app.Run();

public sealed record SensorIngestRequest(Guid DeviceId, string SensorType, double Value, DateTime? TimestampUtc);
public sealed record SensorReadingResponse(DateTime TimestampUtc, double Value, string SensorType);





