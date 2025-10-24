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
using Quasar.Logging;
using Serilog.Events;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.Host.UseQuasarSerilog(configuration, "Logging", options =>
{
    options.UseConsole = true;
    options.UseFile = true;
    options.FilePath = Path.Combine(AppContext.BaseDirectory, "logs", "quasar.log");
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
    ("identity.role_permission_revoked", typeof(Quasar.Identity.RolePermissionRevoked))
});
services.AddQuasarEventSerializer(typeMap);

// Swagger
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

// Identity core services (must be registered before Build)
services.AddQuasarIdentity(o =>
{
    o.Key = "sample-super-secret-key-should-be-long";
    o.Issuer = "quasar"; o.Audience = "quasar";
});

var storeMode = configuration["Quasar:Store"]?.ToLowerInvariant() ?? "inmemory";
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
        services.AddPollingProjector("MainProjector", new[] { SampleConfig.CounterStreamId, SampleConfig.CartStreamId }, TimeSpan.FromMilliseconds(500));
        // Identity read models
        services.UseEfCoreSqlServerReadModels<IdentityReadModelContext>(sqlConnString);
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
        services.AddPollingProjector("MainProjector", new[] { SampleConfig.CounterStreamId, SampleConfig.CartStreamId }, TimeSpan.FromMilliseconds(500));
        // Identity read models
        services.UseEfCoreSqliteReadModels<IdentityReadModelContext>(sqliteConnString);
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
        services.AddPollingProjector("MainProjector", new[] { SampleConfig.CounterStreamId, SampleConfig.CartStreamId }, TimeSpan.FromMilliseconds(500));
        // Identity read models
        services.UseEfCoreSqliteReadModels<IdentityReadModelContext>(demoSqlite);
        break;
}

// Handlers and validators
services.AddScoped<ICommandHandler<IncrementCounterCommand, int>, IncrementCounterHandler>();
services.AddScoped<IQueryHandler<GetCounterQuery, int>, GetCounterHandler>();
services.AddScoped<IValidator<IncrementCounterCommand>, IncrementCounterValidator>();
services.AddScoped<ICommandHandler<AddCartItemCommand, int>, AddCartItemHandler>();
services.AddScoped<IQueryHandler<GetCartQuery, CartReadModel>, GetCartHandler>();
services.AddScoped<IValidator<AddCartItemCommand>, AddCartItemValidator>();

// Authorization service

var app = builder.Build();

// Apply EF Core migrations for read models
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetService<SampleReadModelContext>();
    if (db is not null) await db.Database.MigrateAsync();
    var idDb = scope.ServiceProvider.GetService<IdentityReadModelContext>();
    if (idDb is not null) await idDb.Database.MigrateAsync();
}

// Identity endpoints
app.MapQuasarIdentityEndpoints();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Quasar Sample API v1");
    options.RoutePrefix = string.Empty; // expose at root
});

// Endpoints
app.MapPost("/counter/increment", async (IMediator mediator, int amount, HttpRequest req) =>
{
    if (amount <= 0) return Results.BadRequest("amount is required");
    var subject = req.Headers.TryGetValue("X-Subject", out var s) && Guid.TryParse(s, out var sid) ? sid : Guid.NewGuid();
    var result = await mediator.Send(new IncrementCounterCommand(subject, amount));
    return Results.Ok(new { count = result });
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
    var subject = req.Headers.TryGetValue("X-Subject", out var s) && Guid.TryParse(s, out var sid) ? sid : Guid.NewGuid();
    var total = await mediator.Send(new AddCartItemCommand(subject, productId, quantity));
    return Results.Ok(new { totalItems = total });
}).WithName("AddCartItem").WithTags("Cart");

app.MapGet("/cart", async (IMediator mediator) =>
{
    var cart = await mediator.Send(new GetCartQuery());
    return Results.Ok(cart);
}).WithName("GetCart").WithTags("Cart");

// Debug endpoints
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
    var counter = await cps.GetCheckpointAsync(counterKey);
    var cart = await cps.GetCheckpointAsync(cartKey);
    return Results.Ok(new { counterKey, counter, cartKey, cart });
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

