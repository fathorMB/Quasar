using System;
using System.Data.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quasar.Sagas.Persistence;
using Quasar.EventSourcing.Sqlite;

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
    private bool _initialized;
    private readonly object _initLock = new();

    public EfSagaRepository(SagaDbContext context)
    {
        _context = context;
        _set = context.Sagas;
    }

    public async Task<TState?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        PrepareContext();

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

        PrepareContext();

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
        PrepareContext();

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

    private void PrepareContext()
    {
        AttachAmbientTransactionIfPresent();
        EnsureCreated();
    }

    private void AttachAmbientTransactionIfPresent()
    {
        if (!SqliteAmbientContextAccessor.TryGet(out var connection, out var transaction))
        {
            return;
        }

        var dbConnection = _context.Database.GetDbConnection();
        if (ReferenceEquals(dbConnection, connection))
        {
            _context.Database.UseTransaction(transaction);
        }
    }

    private void EnsureCreated()
    {
        if (_initialized)
        {
            return;
        }

        lock (_initLock)
        {
            if (_initialized)
            {
                return;
            }

            _context.Database.EnsureCreated();
            EnsureSagaTableExists(_context);
            _initialized = true;
        }
    }

    private static void EnsureSagaTableExists(SagaDbContext context)
    {
        var provider = context.Database.ProviderName;
        if (string.IsNullOrWhiteSpace(provider))
        {
            return;
        }

        if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            const string sql = """
CREATE TABLE IF NOT EXISTS QuasarSagaStates (
    Id TEXT NOT NULL PRIMARY KEY,
    SagaType TEXT NOT NULL,
    StateType TEXT NOT NULL,
    Payload TEXT NOT NULL,
    UpdatedUtc TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS IX_QuasarSagaStates_SagaType ON QuasarSagaStates (SagaType);
CREATE INDEX IF NOT EXISTS IX_QuasarSagaStates_StateType ON QuasarSagaStates (StateType);
""";
            context.Database.ExecuteSqlRaw(sql);
            return;
        }

        if (provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            const string sql = """
IF OBJECT_ID(N'[dbo].[QuasarSagaStates]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[QuasarSagaStates](
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [SagaType] NVARCHAR(256) NOT NULL,
        [StateType] NVARCHAR(256) NOT NULL,
        [Payload] NVARCHAR(MAX) NOT NULL,
        [UpdatedUtc] DATETIMEOFFSET NOT NULL
    );
    CREATE INDEX IX_QuasarSagaStates_SagaType ON [dbo].[QuasarSagaStates] ([SagaType]);
    CREATE INDEX IX_QuasarSagaStates_StateType ON [dbo].[QuasarSagaStates] ([StateType]);
END
""";
            context.Database.ExecuteSqlRaw(sql);
        }
    }
}
