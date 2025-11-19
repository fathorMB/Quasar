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
using Quasar.Ui;
using Quasar.Ui.Branding;
using Quasar.Ui.Security;
using Quasar.Web;

namespace BEAM.Server;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;
        var services = builder.Services;

        services.AddQuasarUi(
            configureNavigation: nav =>
            {
                nav.ClearSections()
                    .AddSection("Menu", section =>
                    {
                        section.AddItem("Overview", typeof(Quasar.Ui.Components.Panels.OverviewPanel), slug: string.Empty, isDefault: true);
                        section.AddItem("Identity", typeof(Quasar.Ui.Components.Panels.PlaceholderPanel), "identity", parameters: new Dictionary<string, object?>
                        {
                            { nameof(Quasar.Ui.Components.Panels.PlaceholderPanel.Title), "Identity" }
                        }, configureChildren: children =>
                        {
                            children.AddChild("Users", typeof(Quasar.Ui.Components.Panels.PlaceholderPanel), "users", new Dictionary<string, object?>
                            {
                                { nameof(Quasar.Ui.Components.Panels.PlaceholderPanel.Title), "Users" }
                            });
                            children.AddChild("Roles", typeof(Quasar.Ui.Components.Panels.PlaceholderPanel), "roles", new Dictionary<string, object?>
                            {
                                { nameof(Quasar.Ui.Components.Panels.PlaceholderPanel.Title), "Roles" }
                            });
                        });
                    });
            },
            configureBranding: branding =>
            {
                branding.ApplicationName = "BEAM v1";
                branding.LogoGlyph = "B";
                branding.Palette = QuasarUiColorPalettes.Grove;
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
            seed.WithUser("operator", "operator@beam.local", "ChangeMe123!", "operator");
        });

        // Registers the Quasar-provided JWT bearer scheme so API clients can authenticate with the same options used by the UI.
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

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        app.MapQuasarIdentityEndpoints();

        app.MapStaticAssets();
        app.MapQuasarUi();

        await app.InitializeReadModelsAsync().ConfigureAwait(false);
        await app.SeedDataAsync().ConfigureAwait(false);
        await app.RunAsync().ConfigureAwait(false);
    }

    private static string ResolveSqliteConnectionString(IConfiguration configuration)
    {
        var configured = configuration.GetConnectionString("BeamIdentitySqlite");
        if (string.IsNullOrWhiteSpace(configured))
        {
            configured = "Data Source=beam.identity.db";
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

    private static JwtOptions ResolveJwtOptions(IConfiguration configuration)
    {
        var options = configuration.GetSection("Identity:Jwt").Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(options.Key))
        {
            options.Key = "beam-development-super-secret-key-change-me";
        }

        return options;
    }

    private static AdminSeedOptions ResolveAdminSeedOptions(IConfiguration configuration)
    {
        var admin = configuration.GetSection("Identity:DefaultAdmin").Get<AdminSeedOptions>() ?? new AdminSeedOptions();
        admin.Username = string.IsNullOrWhiteSpace(admin.Username) ? "admin" : admin.Username.Trim();
        admin.Email = string.IsNullOrWhiteSpace(admin.Email) ? "admin@beam.local" : admin.Email.Trim();
        admin.Password = string.IsNullOrWhiteSpace(admin.Password) ? "ChangeMe123!" : admin.Password;
        admin.RoleName = string.IsNullOrWhiteSpace(admin.RoleName) ? "administrator" : admin.RoleName.Trim();
        if (admin.Permissions.Count == 0)
        {
            admin.Permissions.Add("identity.manage");
        }

        return admin;
    }

    private sealed class AdminSeedOptions
    {
        public string Username { get; set; } = "admin";
        public string Email { get; set; } = "admin@beam.local";
        public string Password { get; set; } = "ChangeMe123!";
        public string RoleName { get; set; } = "administrator";
        public IList<string> Permissions { get; set; } = new List<string> { "identity.manage" };
    }

    private static IEventTypeMap CreateIdentityEventTypeMap()
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
}
