using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Quasar.Features;
using Microsoft.Data.Sqlite;
using Quasar.Identity.Persistence.Relational.EfCore.Seeding;
using Quasar.Identity.Web;
using Quasar.Logging;
using Quasar.Persistence.Relational.EfCore;
using Quasar.Projections.Sqlite;
using Quasar.Seeding;
using Quasar.Web;
using Quasar.Discovery;
using Quasar.Scheduling.Quartz;
using Quasar.Telemetry;
using Quartz;
using BEAM.App.Jobs;
using BEAM.App.Domain.Devices;
using BEAM.App.Handlers.Devices;
using BEAM.App.Validators;
using BEAM.App.ReadModels;
using BEAM.App.Projections;
using Quasar.EventSourcing.Abstractions;
using Quasar.Cqrs;
using Quasar.Core;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

var featureRegistry = new FeatureRegistry();
var sqliteConnectionString = ResolveSqliteConnectionString(configuration.GetConnectionString("QuasarSqlite"));

// Register features for UI menu visibility
featureRegistry.Add(new FeatureInfo("scheduler", "Job Scheduler", "Infrastructure", "Quartz.NET job scheduling and management"));
featureRegistry.Add(new FeatureInfo("telemetry", "Telemetry & Metrics", "Monitoring", "Application telemetry and performance metrics"));

services.AddSingleton(featureRegistry);
builder.Host.UseQuasarSerilog(configuration, "Logging", opts =>
{
    opts.UseInMemoryBuffer = true;
    opts.InMemoryBufferCapacity = 512;
    opts.UseConsole = true;
});
services.AddSingleton<InMemoryLogBuffer>();

services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

services.AddAuthorization();

services.AddQuasar(q =>
{
    q.AddMediator();
    q.AddEventSourcingCore();
    q.AddProjections(AppDomain.CurrentDomain.GetAssemblies());
});
services.AddQuasarTelemetry();
services.AddQuasarMetrics();

// Configure UI settings to load custom BEAM bundle
services.AddSingleton(new UiSettings
{
    ApplicationName = "BEAM",
    Theme = "dark",
    LogoSymbol = "B",
    CustomBundleUrl = "/beam-ui/beam-ui.js"
});

// Event type mapping for device and identity events
IEventTypeMap typeMap = new DictionaryEventTypeMap(new[]
{
    ("device.registered", typeof(DeviceRegistered)),
    ("device.activated", typeof(DeviceActivated)),
    ("device.deactivated", typeof(DeviceDeactivated)),
    ("device.connection_state_changed", typeof(DeviceConnectionStateChanged)),
    // Identity events
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

// Identity infrastructure - event store, read models, projections
// Identity infrastructure - event store, read models, projections
services.AddQuasarIdentitySqliteInfrastructure(sqliteConnectionString);

services.UseEfCoreSqliteReadModels<BeamReadModelStore>(sqliteConnectionString);
services.UseSqliteProjectionCheckpoints(() => new SqliteConnection(sqliteConnectionString));

services.AddScoped<object, DeviceProjection>();
services.AddPollingProjector("DeviceProjector", new[] { DeviceConstants.DeviceStreamId }, TimeSpan.FromMilliseconds(500));

// Device command handlers
services.AddScoped<ICommandHandler<RegisterDeviceCommand, Result<Guid>>, RegisterDeviceHandler>();
services.AddScoped<ICommandHandler<ActivateDeviceCommand, Result>, ActivateDeviceHandler>();
services.AddScoped<ICommandHandler<UpdateDeviceConnectionStateCommand, Result>, UpdateDeviceConnectionStateHandler>();

// Device query handlers
services.AddScoped<IQueryHandler<GetDeviceQuery, DeviceReadModel?>, GetDeviceHandler>();
services.AddScoped<IQueryHandler<ListDevicesQuery, PagedResult<DeviceReadModel>>, ListDevicesHandler>();

// Device validators
services.AddScoped<IValidator<RegisterDeviceCommand>, RegisterDeviceValidator>();

var jwtOptions = ResolveJwtOptions(configuration);
services.AddQuasarIdentity(options =>
{
    options.Issuer = jwtOptions.Issuer;
    options.Audience = jwtOptions.Audience;
    options.Key = jwtOptions.Key;
    options.AccessMinutes = jwtOptions.AccessMinutes;
    options.RefreshDays = jwtOptions.RefreshDays;
});
services.AddQuasarIdentitySeed(seed =>
{
    var admin = ResolveAdminSeedOptions(configuration);

    seed.WithRole(admin.RoleName, role =>
    {
        foreach (var permission in admin.Permissions)
        {
            role.Permissions.Add(permission);
        }
    });

    seed.WithRole("operator");

    seed.WithUser(admin.Username, admin.Email, admin.Password, admin.RoleName);
    seed.WithUser("operator", "operator@quasar.local", "ChangeMe123!", "operator");
});

services.AddQuasarJwtAuthentication(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
        ValidateLifetime = true
    };
});

services.AddQuasarUi(ui =>
{
    ui.ApplicationName = "BEAM";
    ui.Theme = "orange";
    ui.LogoSymbol = "B";
    ui.CustomBundleUrl = "/beam-ui/beam-ui.js";
});
services.AddTransient<HeartbeatJob>();
services.AddQuartzScheduler(options =>
{
    options.SchedulerName = "beam-scheduler";
    options.Configure = builder =>
    {
        builder.ScheduleQuasarJob<HeartbeatJob>(
            job => job.WithIdentity("heartbeat", "beam"),
            trigger => trigger
                .WithIdentity("heartbeat-trigger", "beam")
                .WithSimpleSchedule(s => s.WithInterval(TimeSpan.FromMinutes(1)).RepeatForever())
                .StartNow());
    };
});

// UDP Network Discovery Service
services.AddQuasarDiscovery(options =>
{
    options.Enabled = true;
    options.Port = 6000;
    options.ServiceName = "BEAM Identity Server";
    options.Metadata = new Dictionary<string, string>
    {
        { "version", "1.0.0" },
        { "description", "BEAM Identity & Access Management Server" },
        { "framework", "Quasar" }
    };
});

var app = builder.Build();
// Discover features from referenced assemblies (attributes + contributors)
featureRegistry.LoadFromAssemblies(AppDomain.CurrentDomain.GetAssemblies(), app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseCors();
app.UseQuasarTelemetry();
app.UseAuthentication();
app.UseMiddleware<SessionValidationMiddleware>();
app.UseAuthorization();
app.MapQuasarIdentityEndpoints();
app.MapQuasarUiEndpoints();
app.MapQuartzEndpoints();
app.MapQuasarMetricsEndpoints();
app.MapQuasarMetricsHub();
app.MapLoggingEndpoints();


// Device API endpoints
app.MapPost("/api/devices/register", async (IMediator mediator, RegisterDeviceRequest request) =>
{
    if (request == null)
        return Results.BadRequest("Request body is required");

    var command = new RegisterDeviceCommand(
        Guid.Empty, // No auth required for device registration in MVP
        request.DeviceId ?? Guid.NewGuid(),
        request.DeviceName ?? "Unknown Device",
        request.DeviceType ?? "Unknown",
        request.MacAddress ?? "00:00:00:00:00:00");

    var result = await mediator.Send(command);
    return result.IsSuccess
        ? Results.Ok(new { deviceId = result.Value })
        : Results.BadRequest(new { error = result.Error?.Message });
})
.WithName("RegisterDevice")
.WithTags("Devices")
.AllowAnonymous();

app.MapPost("/api/devices/heartbeat", async (IMediator mediator, HeartbeatRequest request) =>
{
    if (request == null || request.DeviceId == Guid.Empty)
        return Results.BadRequest("DeviceId is required");

    var command = new UpdateDeviceConnectionStateCommand(
        Guid.Empty, // anonymous
        request.DeviceId,
        request.IsConnected ?? true);

    var result = await mediator.Send(command);
    return result.IsSuccess
        ? Results.Ok()
        : Results.BadRequest(new { error = result.Error?.Message });
})
.WithName("DeviceHeartbeat")
.WithTags("Devices")
.AllowAnonymous();

app.MapGet("/api/devices", async (IMediator mediator, int page = 1, int pageSize = 20) =>
{
    var result = await mediator.Send(new ListDevicesQuery(page, pageSize));
    return Results.Ok(result);
})
.WithName("ListDevices")
.WithTags("Devices");

app.MapGet("/api/devices/{id:guid}", async (IMediator mediator, Guid id) =>
{
    var result = await mediator.Send(new GetDeviceQuery(id));
    return result != null ? Results.Ok(result) : Results.NotFound();
})
.WithName("GetDevice")
.WithTags("Devices");

app.UseQuasarReactUi();

// Initialize Read Models (Devices)
await app.InitializeReadModelsAsync().ConfigureAwait(false);

await app.SeedDataAsync().ConfigureAwait(false);
await app.RunAsync().ConfigureAwait(false);

static JwtOptions ResolveJwtOptions(IConfiguration configuration)
{
    var options = configuration.GetSection("Identity:Jwt").Get<JwtOptions>() ?? new JwtOptions();
    if (string.IsNullOrWhiteSpace(options.Key))
    {
        options.Key = "quasar-development-super-secret-key-change-me";
    }

    return options;
}

static string ResolveSqliteConnectionString(string connectionString)
{
    var builder = new SqliteConnectionStringBuilder
    {
        ConnectionString = connectionString
    };

    var dataSource = builder.DataSource;
    if (!Path.IsPathRooted(dataSource))
    {
        dataSource = Path.Combine(AppContext.BaseDirectory, dataSource);
    }

    var directory = Path.GetDirectoryName(dataSource);
    if (!string.IsNullOrEmpty(directory))
    {
        Directory.CreateDirectory(directory);
    }

    builder.DataSource = Path.GetFullPath(dataSource);
    builder.Mode = SqliteOpenMode.ReadWriteCreate;

    return builder.ToString();
}

static AdminSeedOptions ResolveAdminSeedOptions(IConfiguration configuration)
{
    var admin = configuration.GetSection("Identity:DefaultAdmin").Get<AdminSeedOptions>() ?? new AdminSeedOptions();
    admin.Username = string.IsNullOrWhiteSpace(admin.Username) ? "admin" : admin.Username.Trim();
    admin.Email = string.IsNullOrWhiteSpace(admin.Email) ? "admin@quasar.local" : admin.Email.Trim();
    admin.Password = string.IsNullOrWhiteSpace(admin.Password) ? "ChangeMe123!" : admin.Password;
    admin.RoleName = string.IsNullOrWhiteSpace(admin.RoleName) ? "administrator" : admin.RoleName.Trim();
    if (admin.Permissions.Count == 0)
    {
        admin.Permissions.Add("identity.manage");
    }

    return admin;
}

sealed class AdminSeedOptions
{
    public string Username { get; set; } = "admin";
    public string Email { get; set; } = "admin@quasar.local";
    public string Password { get; set; } = "ChangeMe123!";
    public string RoleName { get; set; } = "administrator";
    public IList<string> Permissions { get; set; } = new List<string> { "identity.manage" };
}

// Request DTOs for device endpoints
sealed record RegisterDeviceRequest(Guid? DeviceId, string? DeviceName, string? DeviceType, string? MacAddress);
sealed record HeartbeatRequest(Guid DeviceId, bool? IsConnected);
