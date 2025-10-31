namespace Quasar.Sagas.Persistence;

/// <summary>
/// Provides persistence operations for saga state.
/// </summary>
/// <typeparam name="TState">Saga state type.</typeparam>
public interface ISagaRepository<TState> where TState : class, ISagaState, new()
{
    Task<TState?> FindAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveAsync(TState state, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
