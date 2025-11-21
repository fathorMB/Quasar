using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Quasar.Discovery;

/// <summary>
/// Extension methods for adding the discovery service to the application.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the UDP network discovery service to the application.
    /// Developers can configure the port, service name, and metadata.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for discovery options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddQuasarDiscovery(
        this IServiceCollection services,
        Action<DiscoveryOptions> configure)
    {
        services.Configure(configure);
        services.AddHostedService<DiscoveryService>();
        return services;
    }

    /// <summary>
    /// Adds the UDP network discovery service with configuration from appsettings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configSection">The configuration section name (default: "Discovery").</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddQuasarDiscovery(
        this IServiceCollection services,
        string configSection = "Discovery")
    {
        services.AddOptions<DiscoveryOptions>()
            .BindConfiguration(configSection);
        services.AddHostedService<DiscoveryService>();
        return services;
    }
}
