using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using Quasar.EventSourcing.Abstractions;
using Quasar.EventSourcing.Sqlite;
using Quasar.Identity;
using Quasar.Identity.Persistence.Relational.EfCore;
using Quasar.Identity.Persistence.Relational.EfCore.Seeding;
using Quasar.Identity.Web;
using Quasar.Persistence.Relational.EfCore;
using Quasar.Seeding;
using Quasar.Web;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

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
services.AddQuasarMediator();
services.AddQuasarEventSourcingCore();
services.AddReadModelDefinition<IdentityReadModelDefinition>();
services.AddQuasarEventSerializer(CreateIdentityEventTypeMap());

var sqliteConnectionString = ResolveSqliteConnectionString(configuration);
var sqliteOptions = new SqliteEventStoreOptions
{
    ConnectionFactory = () => new SqliteConnection(sqliteConnectionString)
};

await SqliteEventStoreInitializer.EnsureSchemaAsync(sqliteOptions).ConfigureAwait(false);
services.UseSqliteEventStore(sqliteOptions);
services.UseSqliteCommandTransaction();
services.UseEfCoreSqliteReadModels<IdentityReadModelStore>(sqliteConnectionString, registerRepositories: false);
services.AddScoped<object, IdentityProjections>();

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

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapQuasarIdentityEndpoints();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

await app.InitializeReadModelsAsync().ConfigureAwait(false);
await app.SeedDataAsync().ConfigureAwait(false);
await app.RunAsync().ConfigureAwait(false);

static string ResolveSqliteConnectionString(IConfiguration configuration)
{
    var configured = configuration.GetConnectionString("QuasarSqlite");
    if (string.IsNullOrWhiteSpace(configured))
    {
        configured = "Data Source=quasar.identity.db";
    }

    var builder = new SqliteConnectionStringBuilder(configured);
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

static JwtOptions ResolveJwtOptions(IConfiguration configuration)
{
    var options = configuration.GetSection("Identity:Jwt").Get<JwtOptions>() ?? new JwtOptions();
    if (string.IsNullOrWhiteSpace(options.Key))
    {
        options.Key = "quasar-development-super-secret-key-change-me";
    }

    return options;
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

static IEventTypeMap CreateIdentityEventTypeMap()
{
    return new DictionaryEventTypeMap(new[]
    {
        ("identity.user_registered", typeof(UserRegistered)),
        ("identity.user_password_set", typeof(UserPasswordSet)),
        ("identity.user_role_assigned", typeof(UserRoleAssigned)),
        ("identity.user_role_revoked", typeof(UserRoleRevoked)),
        ("identity.role_created", typeof(RoleCreated)),
        ("identity.role_renamed", typeof(RoleRenamed)),
        ("identity.role_permission_granted", typeof(RolePermissionGranted)),
        ("identity.role_permission_revoked", typeof(RolePermissionRevoked))
    });
}

sealed class AdminSeedOptions
{
    public string Username { get; set; } = "admin";
    public string Email { get; set; } = "admin@quasar.local";
    public string Password { get; set; } = "ChangeMe123!";
    public string RoleName { get; set; } = "administrator";
    public IList<string> Permissions { get; set; } = new List<string> { "identity.manage" };
}
