namespace Quasar.Persistence.Abstractions;

public interface IReadRepository<T>
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}

public interface IDocumentCollection<TDocument>
{
    Task<TDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpsertAsync(TDocument document, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IRelationalQuery<T>
{
    IQueryable<T> Queryable { get; }
}

