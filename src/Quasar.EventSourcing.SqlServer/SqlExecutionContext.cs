using System.Data.Common;

namespace Quasar.EventSourcing.SqlServer;

internal sealed class AmbientSqlContext
{
    public required DbConnection Connection { get; init; }
    public required DbTransaction Transaction { get; init; }
}

internal static class SqlExecutionContext
{
    private static readonly AsyncLocal<AmbientSqlContext?> _current = new();
    public static AmbientSqlContext? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}

