using Microsoft.EntityFrameworkCore;
using Quasar.Persistence.Abstractions;

namespace Quasar.Persistence.Relational.EfCore;

/// <summary>
/// Base EF Core <see cref="DbContext"/> used for projection/read model storage.
/// </summary>
public abstract class ReadModelContext : DbContext
{
    private readonly IReadModelModelSource _modelSource;
    private readonly Type _storeKey;

    protected ReadModelContext(
        DbContextOptions options,
        IReadModelModelSource modelSource,
        Type storeKey) : base(options)
    {
        _modelSource = modelSource;
        _storeKey = storeKey;
    }

    /// <summary>
    /// Applies registered read model definitions for the configured store.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _modelSource.Configure(modelBuilder, _storeKey);
    }
}

/// <summary>
/// Entity Framework based implementation of <see cref="IReadRepository{T}"/>.
/// </summary>
public class EfReadRepository<T> : IReadRepository<T> where T : class
{
    private readonly ReadModelContext _db;
    private readonly DbSet<T> _set;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfReadRepository{T}"/> class.
    /// </summary>
    public EfReadRepository(ReadModelContext db)
    {
        _db = db;
        _set = _db.Set<T>();
    }

    /// <inheritdoc />
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _set.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _set.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _set.FindAsync([id], cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _set.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _set.Update(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
