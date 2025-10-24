namespace Quasar.RealTime;

/// <summary>
/// Dispatches payloads to real-time subscribers.
/// </summary>
/// <typeparam name="TPayload">Type of payload being broadcast.</typeparam>
public interface IRealTimeNotifier<TPayload>
{
    /// <summary>
    /// Broadcasts the provided <paramref name="payload"/> to all connected listeners.
    /// </summary>
    Task NotifyAsync(TPayload payload, CancellationToken cancellationToken = default);
}
