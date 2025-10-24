using System.Data.Common;

namespace Quasar.EventSourcing.Sqlite;

internal sealed class AmbientSqliteContext
{
    public required DbConnection Connection { get; init; }
    public required DbTransaction Transaction { get; init; }
}

internal static class SqliteExecutionContext
{
    private static readonly AsyncLocal<AmbientSqliteContext?> _current = new();
    public static AmbientSqliteContext? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}

