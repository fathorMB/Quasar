using Microsoft.Extensions.DependencyInjection;
using Quasar.Cqrs;

namespace Quasar.Auditing;

/// <summary>
/// Service collection extensions for enabling command auditing.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Quasar Auditing pipeline behavior and a default logging-based audit store.
    /// </summary>
    public static IServiceCollection AddQuasarAuditing(this IServiceCollection services)
    {
        services.AddTransient<IAuditStore, LoggingAuditStore>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
        return services;
    }
}
