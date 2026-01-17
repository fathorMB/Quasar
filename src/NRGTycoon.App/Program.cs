using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
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
using Quasar.EventSourcing.Abstractions;
using Quasar.Cqrs;
using Quasar.Core;
using NRGTycoon.App.Domain.Companies;
using NRGTycoon.App.Handlers.Companies;
using NRGTycoon.App.ReadModels;
using NRGTycoon.App.Projections;
using Quasar.Projections.Abstractions;
using NRGTycoon.App;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

var featureRegistry = new FeatureRegistry();
var sqliteConnectionString = ResolveSqliteConnectionString(configuration.GetConnectionString("QuasarSqlite"));

services.AddSingleton(featureRegistry);
builder.Host.UseQuasarSerilog(configuration, "Logging", opts =>
{
    opts.UseInMemoryBuffer = true;
    opts.InMemoryBufferCapacity = 256;
    opts.UseConsole = true;
});
services.AddSingleton<InMemoryLogBuffer>();

services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:5175", "http://localhost:5292")
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

// UI settings are configured below in AddQuasarUi

// Event type mapping - identity + company events
IEventTypeMap typeMap = new DictionaryEventTypeMap(new[]
{
    // Identity events
    ("identity.user_registered", typeof(Quasar.Identity.UserRegistered)),
    ("identity.user_password_set", typeof(Quasar.Identity.UserPasswordSet)),
    ("identity.user_role_assigned", typeof(Quasar.Identity.UserRoleAssigned)),
    ("identity.user_role_revoked", typeof(Quasar.Identity.UserRoleRevoked)),
    ("identity.role_created", typeof(Quasar.Identity.RoleCreated)),
    ("identity.role_renamed", typeof(Quasar.Identity.RoleRenamed)),
    ("identity.role_permission_granted", typeof(Quasar.Identity.RolePermissionGranted)),
    ("identity.role_permission_revoked", typeof(Quasar.Identity.RolePermissionRevoked)),
    // Company events
    ("company.created", typeof(CompanyCreated)),
    ("company.name_updated", typeof(CompanyNameUpdated)),
    ("company.balance_movement_recorded", typeof(BalanceMovementRecorded))
});
services.AddQuasarEventSerializer(typeMap);

// Identity infrastructure - event store, read models, projections
services.AddQuasarIdentitySqliteInfrastructure(sqliteConnectionString);

// Game read models
// Game read models
var readModelConnectionString = ResolveSqliteConnectionString("Data Source=data/nrgtycoon_read.db");
services.UseEfCoreSqliteReadModels<NRGTycoonReadModelStore>(readModelConnectionString);
services.UseSqliteProjectionCheckpoints(() => new SqliteConnection(sqliteConnectionString));

// Add polling projector for company events (for potential catch-ups)
// Polling projector restored to populate new Read Model DB
services.AddPollingProjector("CompanyProjector", Array.Empty<Guid>(), TimeSpan.FromMilliseconds(500));

// Add live projections for real-time read model updates
services.AddLiveProjections();
services.AddLiveProjection<CompanyProjection, CompanyCreated>();
services.AddLiveProjection<CompanyProjection, CompanyNameUpdated>();
services.AddLiveProjection<CompanyProjection, BalanceMovementRecorded>();

// Command handlers
services.AddScoped<ICommandHandler<CreateCompanyCommand, Result<Guid>>, CreateCompanyHandler>();
services.AddScoped<ICommandHandler<UpdateCompanyNameCommand, Result>, UpdateCompanyNameHandler>();
services.AddScoped<ICommandHandler<RecordBalanceMovementCommand, Result>, RecordBalanceMovementHandler>();

// Query handlers
services.AddScoped<IQueryHandler<GetCompanyDashboardQuery, CompanyDashboardDto?>, GetCompanyDashboardHandler>();

var jwtOptions = ResolveJwtOptions(configuration);
services.AddSingleton(jwtOptions);
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

    // Player role - for regular game players
    seed.WithRole("player", role =>
    {
        role.Permissions.Add("game.play");
    });

    seed.WithUser(admin.Username, admin.Email, admin.Password, admin.RoleName);
    seed.WithUser("player", "player@nrgtycoon.local", "ChangeMe123!", "player");
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
    
    options.IncludeErrorDetails = true;
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("JWT Auth Failed: {Message} ({ExceptionType})", context.Exception.Message, context.Exception.GetType().Name);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
             var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
             logger.LogInformation("JWT Validated. User: {User}, Claims: {Claims}", 
                context.Principal?.Identity?.Name, 
                string.Join(", ", context.Principal?.Claims.Select(c => c.Type + "=" + c.Value) ?? Array.Empty<string>()));
             return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT Challenge: {Error} - {Desc}. Failure: {Fail}", 
                context.Error, context.ErrorDescription, context.AuthenticateFailure?.Message);
            return Task.CompletedTask;
        }
    };
});

services.AddQuasarUi(ui =>
{
    ui.ApplicationName = "NRG Tycoon";
    ui.Theme = "dark";
    ui.LogoSymbol = "⚡";
    ui.CustomBundleUrl = "/nrg-ui/nrg-ui.js";
});

var app = builder.Build();

app.MapGet("/api/debug/me", (ClaimsPrincipal user) => 
{
    return Results.Ok(user.Claims.Select(c => new { c.Type, c.Value }));
}).RequireAuthorization();
app.Services.ConfigureLiveProjections();
featureRegistry.LoadFromAssemblies(AppDomain.CurrentDomain.GetAssemblies(), app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseMiddleware<SessionValidationMiddleware>();
app.UseAuthorization();
app.MapQuasarIdentityEndpoints();
app.MapQuasarUiEndpoints();
app.MapLoggingEndpoints();

// ========== Company API Endpoints ==========

// Create company (first-time setup)
app.MapPost("/api/company", async (IMediator mediator, ClaimsPrincipal user, CreateCompanyRequest request, ILogger<Program> logger) =>
{
    logger.LogInformation("POST /api/company called. User: {User}, Name: {Name}", user.Identity?.Name, request.Name);
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var ownerId))
        return Results.Unauthorized();

    var result = await mediator.Send(new CreateCompanyCommand(ownerId, request.Name));
    return result.IsSuccess
        ? Results.Ok(new { companyId = result.Value })
        : Results.BadRequest(new { error = result.Error?.Message });
})
.WithName("CreateCompany")
.WithTags("Company")
.RequireAuthorization();

// Update company name
app.MapPut("/api/company/name", async (IMediator mediator, ClaimsPrincipal user, UpdateCompanyNameRequest request) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var ownerId))
        return Results.Unauthorized();

    var result = await mediator.Send(new UpdateCompanyNameCommand(ownerId, request.NewName));
    return result.IsSuccess
        ? Results.Ok()
        : Results.BadRequest(new { error = result.Error?.Message });
})
.WithName("UpdateCompanyName")
.WithTags("Company")
.RequireAuthorization();

// Get company dashboard
app.MapGet("/api/company/dashboard", async (IMediator mediator, ClaimsPrincipal user) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var ownerId))
        return Results.Unauthorized();

    var dashboard = await mediator.Send(new GetCompanyDashboardQuery(ownerId));
    return dashboard != null
        ? Results.Ok(dashboard)
        : Results.NotFound(new { message = "Company not found. Please create your company first." });
})
.WithName("GetCompanyDashboard")
.WithTags("Company")
.RequireAuthorization();

app.UseQuasarReactUi();

// Initialize Read Models (schema creation)
await app.InitializeReadModelsAsync().ConfigureAwait(false);

await app.SeedDataAsync().ConfigureAwait(false);
await app.RunAsync().ConfigureAwait(false);

// ========== Helper Methods ==========

static JwtOptions ResolveJwtOptions(IConfiguration configuration)
{
    var options = configuration.GetSection("Identity:Jwt").Get<JwtOptions>() ?? new JwtOptions();
    if (string.IsNullOrWhiteSpace(options.Key))
    {
        options.Key = "nrg-tycoon-development-super-secret-key-change-me";
    }

    return options;
}

static string ResolveSqliteConnectionString(string? connectionString)
{
    connectionString ??= "Data Source=data/nrgtycoon.db";
    
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
    admin.Email = string.IsNullOrWhiteSpace(admin.Email) ? "admin@nrgtycoon.local" : admin.Email.Trim();
    admin.Password = string.IsNullOrWhiteSpace(admin.Password) ? "ChangeMe123!" : admin.Password;
    admin.RoleName = string.IsNullOrWhiteSpace(admin.RoleName) ? "administrator" : admin.RoleName.Trim();
    if (admin.Permissions.Count == 0)
    {
        admin.Permissions.Add("identity.manage");
        admin.Permissions.Add("game.admin");
    }

    return admin;
}
