namespace Quasar.Auditing;

/// <summary>
/// Represents a record of an executed command for auditing purposes.
/// </summary>
/// <param name="Timestamp">When the command was executed.</param>
/// <param name="SubjectId">The user or system that initiated the command.</param>
/// <param name="CommandType">The CLR type name of the command.</param>
/// <param name="CommandJson">The full command payload serialized as JSON.</param>
/// <param name="IsSuccess">Whether the command pipeline completed without an exception.</param>
/// <param name="Error">The error message if the command failed.</param>
public sealed record AuditEntry(
    DateTime Timestamp,
    Guid? SubjectId,
    string CommandType,
    string CommandJson,
    bool IsSuccess,
    string? Error);

/// <summary>
/// Defines a storage mechanism for audit entries.
/// </summary>
public interface IAuditStore
{
    /// <summary>
    /// Persists an audit entry.
    /// </summary>
    /// <param name="entry">The entry to store.</param>
    Task StoreAsync(AuditEntry entry);
}
