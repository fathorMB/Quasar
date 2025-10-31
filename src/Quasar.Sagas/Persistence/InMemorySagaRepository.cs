using System.Collections.Concurrent;

namespace Quasar.Sagas.Persistence;

/// <summary>
/// In-memory saga repository suitable for development and testing.
/// </summary>
public sealed class InMemorySagaRepository<TState> : ISagaRepository<TState>
    where TState : class, ISagaState, new()
{
    private readonly ConcurrentDictionary<Guid, TState> _store = new();

    public Task<TState?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var state);
        return Task.FromResult(state);
    }

    public Task SaveAsync(TState state, CancellationToken cancellationToken = default)
    {
        if (state is null) throw new ArgumentNullException(nameof(state));
        state.UpdatedUtc = DateTimeOffset.UtcNow;
        _store[state.Id] = state;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
