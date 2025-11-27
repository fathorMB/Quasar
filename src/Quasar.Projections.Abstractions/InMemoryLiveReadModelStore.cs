using System.Collections.Concurrent;

namespace Quasar.Projections.Abstractions;

/// <summary>
/// In-memory implementation of live read model store suitable for development and small deployments.
/// </summary>
public sealed class InMemoryLiveReadModelStore : ILiveReadModelStore
{
    private readonly ConcurrentDictionary<string, object> _store = new();

    /// <inheritdoc />
    public Task UpsertAsync<TReadModel>(string key, TReadModel model, CancellationToken cancellationToken = default)
        where TReadModel : class
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(model);

        _store.AddOrUpdate(key, model, (_, _) => model);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<TReadModel?> GetAsync<TReadModel>(string key, CancellationToken cancellationToken = default)
        where TReadModel : class
    {
        ArgumentNullException.ThrowIfNull(key);

        if (_store.TryGetValue(key, out var value) && value is TReadModel model)
            return Task.FromResult<TReadModel?>(model);

        return Task.FromResult<TReadModel?>(null);
    }

    /// <inheritdoc />
    public Task DeleteAsync<TReadModel>(string key, CancellationToken cancellationToken = default)
        where TReadModel : class
    {
        ArgumentNullException.ThrowIfNull(key);

        _store.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TReadModel>> GetAllAsync<TReadModel>(CancellationToken cancellationToken = default)
        where TReadModel : class
    {
        var result = _store.Values
            .OfType<TReadModel>()
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<TReadModel>>(result);
    }
}
