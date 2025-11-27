using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.Projections.Abstractions;

/// <summary>
/// Manages dispatch of events to live projection handlers.
/// </summary>
public sealed class LiveProjectionPipeline : ILiveProjectionPipeline
{
    private readonly IServiceProvider _provider;
    private readonly ConcurrentDictionary<Type, List<Type>> _handlers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveProjectionPipeline"/> class.
    /// </summary>
    /// <param name="provider">Service provider for resolving projection instances.</param>
    public LiveProjectionPipeline(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public void RegisterProjection(Type eventType, Type projectionType)
    {
        if (!typeof(IEvent).IsAssignableFrom(eventType))
            throw new ArgumentException($"Event type must implement IEvent", nameof(eventType));

        _handlers.AddOrUpdate(
            eventType,
            new List<Type> { projectionType },
            (_, list) => { list.Add(projectionType); return list; });
    }

    /// <inheritdoc />
    public async Task DispatchAsync(EventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        var eventType = envelope.Event.GetType();

        if (!_handlers.TryGetValue(eventType, out var projectionTypes))
            return;

        using var scope = _provider.CreateScope();

        foreach (var projType in projectionTypes)
        {
            var projection = scope.ServiceProvider.GetService(projType);
            if (projection is null)
                continue;

            var method = FindHandleMethod(projType, eventType);
            if (method is null)
                continue;

            try
            {
                var task = (Task?)method.Invoke(projection, new object?[] { envelope.Event, cancellationToken });
                if (task is not null)
                    await task.ConfigureAwait(false);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }
    }

    private static MethodInfo? FindHandleMethod(Type projectionType, Type eventType)
    {
        return projectionType.GetMethods()
            .Where(m => m.Name == "HandleAsync")
            .FirstOrDefault(m =>
            {
                var parameters = m.GetParameters();
                return parameters.Length == 2
                    && parameters[0].ParameterType.IsAssignableFrom(eventType)
                    && parameters[1].ParameterType == typeof(CancellationToken);
            });
    }
}
