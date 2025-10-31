using System.Collections.Concurrent;

namespace Quasar.Samples.BasicApi;

/// <summary>
/// Captures the latest observable state of checkout sagas so UI clients can inspect completed workflows.
/// </summary>
public sealed class CheckoutSagaMonitor
{
    private readonly ConcurrentDictionary<Guid, CheckoutStatusResponse> _snapshots = new();

    public void Record(CheckoutSagaState state)
    {
        if (state is null) return;
        var snapshot = CheckoutStatusResponse.FromState(state);
        Track(snapshot, state.Id);
        if (state.CheckoutId != Guid.Empty)
        {
            Track(snapshot, state.CheckoutId);
        }
    }

    public CheckoutStatusResponse? TryGet(Guid checkoutId)
        => _snapshots.TryGetValue(checkoutId, out var snapshot) ? snapshot : null;

    public void RecordSnapshot(CheckoutStatusResponse snapshot)
    {
        if (snapshot is null) return;
        Track(snapshot, snapshot.CheckoutId);
    }

    private void Track(CheckoutStatusResponse snapshot, Guid key)
    {
        if (key == Guid.Empty) return;
        _snapshots[key] = snapshot;
    }
}
