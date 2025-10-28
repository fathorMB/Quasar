using Microsoft.EntityFrameworkCore;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.EventSourcing.Outbox.EfCore;

/// <summary>
/// EF Core backed inbox store for idempotent message processing.
/// </summary>
public sealed class EfCoreInboxStore : IInboxStore
{
    private readonly OutboxDbContext _context;

    public EfCoreInboxStore(OutboxDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TryEnsureProcessedAsync(InboxMessage message, CancellationToken cancellationToken = default)
    {
        var alreadyProcessed = await _context.InboxMessages
            .AsNoTracking()
            .AnyAsync(x => x.Source == message.Source && x.MessageId == message.MessageId, cancellationToken)
            .ConfigureAwait(false);

        if (alreadyProcessed)
        {
            return false;
        }

        var entity = new InboxMessageEntity
        {
            Source = message.Source,
            MessageId = message.MessageId,
            Hash = message.Hash,
            ReceivedUtc = message.ReceivedUtc,
            ProcessedUtc = message.ProcessedUtc
        };

        _context.InboxMessages.Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _context.Entry(entity).State = EntityState.Detached;
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is not null)
        {
            _context.Entry(entity).State = EntityState.Detached;
            return false;
        }
    }

    public async Task PurgeAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default)
    {
        await _context.InboxMessages
            .Where(x => x.ReceivedUtc <= cutoffUtc)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
