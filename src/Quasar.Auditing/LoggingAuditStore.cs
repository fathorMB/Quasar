using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Quasar.Auditing;

/// <summary>
/// An implementation of IAuditStore that writes audit entries to the configured ILogger.
/// </summary>
public sealed class LoggingAuditStore : IAuditStore
{
    private readonly ILogger<LoggingAuditStore> _logger;

    public LoggingAuditStore(ILogger<LoggingAuditStore> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StoreAsync(AuditEntry entry)
    {
        if (entry.IsSuccess)
        {
            _logger.LogInformation(
                "Audit: Command {CommandType} succeeded for subject {SubjectId}. Command Body: {CommandJson}",
                entry.CommandType, entry.SubjectId, entry.CommandJson);
        }
        else
        {
            _logger.LogWarning(
                "Audit: Command {CommandType} failed for subject {SubjectId}. Error: {Error}. Command Body: {CommandJson}",
                entry.CommandType, entry.SubjectId, entry.Error, entry.CommandJson);
        }

        return Task.CompletedTask;
    }
}
