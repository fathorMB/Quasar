namespace Quasar.Persistence.Abstractions;

/// <summary>
/// Defines read-only access to projection-backed models.
/// </summary>
/// <typeparam name="T">Type of entity returned by the repository.</typeparam>
public interface IReadRepository<T>
{
    /// <summary>
    /// Retrieves an entity by its identifier.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all entities tracked by the repository.
    /// </summary>
    Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the read store.
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the read store.
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the given entity from the read store.
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines CRUD operations for document-oriented stores.
/// </summary>
/// <typeparam name="TDocument">Document type being stored.</typeparam>
public interface IDocumentCollection<TDocument>
{
    /// <summary>
    /// Retrieves a document by its identifier.
    /// </summary>
    Task<TDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates the provided <paramref name="document"/>.
    /// </summary>
    Task UpsertAsync(TDocument document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a document by its identifier.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a composable query against a relational read model.
/// </summary>
/// <typeparam name="T">Query element type.</typeparam>
public interface IRelationalQuery<T>
{
    /// <summary>
    /// Gets the <see cref="IQueryable{T}"/> that can be used to compose additional filters.
    /// </summary>
    IQueryable<T> Queryable { get; }
}
