using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quasar.Sagas.Persistence;

namespace Quasar.Sagas.Persistence.Relational.EfCore;

internal sealed class EfSagaRepository<TState> : ISagaRepository<TState>
    where TState : class, ISagaState, new()
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly SagaDbContext _context;
    private readonly DbSet<SagaRecord> _set;

    public EfSagaRepository(SagaDbContext context)
    {
        _context = context;
        _set = context.Sagas;
    }

    public async Task<TState?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _set.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (record is null)
        {
            return null;
        }

        return Deserialize(record.Payload);
    }

    public async Task SaveAsync(TState state, CancellationToken cancellationToken = default)
    {
        if (state is null) throw new ArgumentNullException(nameof(state));

        var record = await _set.FirstOrDefaultAsync(x => x.Id == state.Id, cancellationToken).ConfigureAwait(false);
        var payload = Serialize(state);
        if (record is null)
        {
            record = new SagaRecord
            {
                Id = state.Id,
                SagaType = state.GetType().DeclaringType?.FullName ?? state.GetType().FullName ?? typeof(TState).FullName ?? typeof(TState).Name,
                StateType = typeof(TState).FullName ?? typeof(TState).Name,
                Payload = payload,
                UpdatedUtc = state.UpdatedUtc
            };
            _set.Add(record);
        }
        else
        {
            record.Payload = payload;
            record.UpdatedUtc = state.UpdatedUtc;
            record.StateType = typeof(TState).FullName ?? typeof(TState).Name;
        }

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _set.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (record is null)
        {
            return;
        }

        _set.Remove(record);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string Serialize(TState state)
        => JsonSerializer.Serialize(state, SerializerOptions);

    private static TState? Deserialize(string payload)
        => JsonSerializer.Deserialize<TState>(payload, SerializerOptions);
}
