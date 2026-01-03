using System.IO;
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
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
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

// Configure UI settings - NRG Tycoon branding
services.AddSingleton(new UiSettings
{
    ApplicationName = "NRG Tycoon",
    Theme = "dark",
    LogoSymbol = "⚡",
    CustomBundleUrl = null // No custom UI bundle yet
});

// Event type mapping - identity events only for now
// Game events will be added later
IEventTypeMap typeMap = new DictionaryEventTypeMap(new[]
{
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
services.AddQuasarIdentitySqliteInfrastructure(sqliteConnectionString);
services.UseSqliteProjectionCheckpoints(() => new SqliteConnection(sqliteConnectionString));

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

    seed.WithUser(admin.Username, admin.Email, admin.Password, admin.RoleName);
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
    ui.ApplicationName = "NRG Tycoon";
    ui.Theme = "dark";
    ui.LogoSymbol = "⚡";
});

var app = builder.Build();
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

app.UseQuasarReactUi();

// Initialize Read Models (schema creation)
await app.InitializeReadModelsAsync().ConfigureAwait(false);

await app.SeedDataAsync().ConfigureAwait(false);
await app.RunAsync().ConfigureAwait(false);

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

sealed class AdminSeedOptions
{
    public string Username { get; set; } = "admin";
    public string Email { get; set; } = "admin@nrgtycoon.local";
    public string Password { get; set; } = "ChangeMe123!";
    public string RoleName { get; set; } = "administrator";
    public IList<string> Permissions { get; set; } = new List<string> { "identity.manage", "game.admin" };
}
