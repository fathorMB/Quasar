using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.EventSourcing.Outbox.EfCore;

/// <summary>
/// Stores outbox messages using an EF Core <see cref="DbContext"/>.
/// </summary>
public sealed class EfCoreOutboxStore : IOutboxStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly OutboxDbContext _context;

    public EfCoreOutboxStore(OutboxDbContext context)
    {
        _context = context;
    }

    public async Task EnqueueAsync(IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken = default)
    {
        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            var entity = new OutboxMessageEntity
            {
                MessageId = message.MessageId,
                StreamId = message.StreamId,
                StreamVersion = message.StreamVersion,
                EventName = message.EventName,
                Payload = message.Payload,
                CreatedUtc = message.CreatedUtc,
                Destination = message.Destination,
                MetadataJson = message.Metadata is { Count: > 0 }
                    ? JsonSerializer.Serialize(message.Metadata, SerializerOptions)
                    : null
            };

            _context.OutboxMessages.Add(entity);
        }

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<OutboxPendingMessage>> GetPendingAsync(int batchSize, int maxAttempts, CancellationToken cancellationToken = default)
    {
        var pending = await _context.OutboxMessages
            .AsNoTracking()
            .Where(x => x.DispatchedUtc == null && x.AttemptCount < maxAttempts)
            .OrderBy(x => x.CreatedUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (pending.Count == 0)
        {
            return Array.Empty<OutboxPendingMessage>();
        }

        var results = new List<OutboxPendingMessage>(pending.Count);
        foreach (var entity in pending)
        {
            results.Add(new OutboxPendingMessage(
                entity.MessageId,
                entity.StreamId,
                entity.StreamVersion,
                entity.EventName,
                entity.Payload,
                entity.CreatedUtc,
                entity.AttemptCount,
                entity.LastAttemptUtc,
                entity.Destination,
                entity.LastError,
                DeserializeMetadata(entity.MetadataJson)));
        }

        return results;
    }

    public async Task RecordDispatchOutcomeAsync(Guid messageId, DateTime attemptUtc, bool succeeded, string? error = null, CancellationToken cancellationToken = default)
    {
        var entity = await _context.OutboxMessages
            .FirstOrDefaultAsync(x => x.MessageId == messageId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return;
        }

        entity.AttemptCount += 1;
        entity.LastAttemptUtc = attemptUtc;

        if (succeeded)
        {
            entity.DispatchedUtc = attemptUtc;
            entity.LastError = null;
        }
        else
        {
            entity.LastError = error;
        }

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static IReadOnlyDictionary<string, string>? DeserializeMetadata(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return null;
        }

        var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson, SerializerOptions);
        return dictionary is { Count: > 0 } ? dictionary : null;
    }
}
