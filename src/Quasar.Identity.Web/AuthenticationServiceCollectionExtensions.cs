using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace Quasar.Identity.Web;

public static class AuthenticationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the default Quasar JWT bearer authentication scheme.
    /// Hosts can override any TokenValidationParameters by supplying <paramref name="configure"/>.
    /// </summary>
    public static IServiceCollection AddQuasarJwtAuthentication(
        this IServiceCollection services,
        Action<JwtBearerOptions>? configure = null,
        string scheme = JwtBearerDefaults.AuthenticationScheme)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = scheme;
                options.DefaultChallengeScheme = scheme;
            })
            .AddJwtBearer(scheme, configure ?? (_ => { }));

        return services;
    }
}
