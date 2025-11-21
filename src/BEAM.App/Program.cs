using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Quasar.Features;
using Quasar.Identity.Persistence.Relational.EfCore.Seeding;
using Quasar.Identity.Web;
using Quasar.Persistence.Relational.EfCore;
using Quasar.Seeding;
using Quasar.Web;
using Quasar.Discovery;
using Quasar.Scheduling.Quartz;
using Quartz;
using BEAM.App.Jobs;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;
var featureRegistry = new FeatureRegistry();
services.AddSingleton(featureRegistry);

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

// Identity infrastructure - event store, read models, projections
services.AddQuasarIdentitySqliteInfrastructure(configuration.GetConnectionString("QuasarSqlite"));

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
app.UseAuthentication();
app.UseAuthorization();
app.MapQuasarIdentityEndpoints();
app.MapQuasarUiEndpoints();
app.MapQuartzEndpoints();

app.UseQuasarReactUi();


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
