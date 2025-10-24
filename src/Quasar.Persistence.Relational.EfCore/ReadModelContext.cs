using Microsoft.EntityFrameworkCore;
using Quasar.Persistence.Abstractions;

namespace Quasar.Persistence.Relational.EfCore;

public abstract class ReadModelContext : DbContext
{
    protected ReadModelContext(DbContextOptions options) : base(options) { }
}

public class EfReadRepository<T> : IReadRepository<T> where T : class
{
    private readonly ReadModelContext _db;
    private readonly DbSet<T> _set;

    public EfReadRepository(ReadModelContext db)
    {
        _db = db;
        _set = _db.Set<T>();
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _set.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _set.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _set.FindAsync([id], cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _set.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _set.Update(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

