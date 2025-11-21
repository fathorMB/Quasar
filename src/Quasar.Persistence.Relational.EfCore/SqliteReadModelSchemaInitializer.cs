using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Quasar.Persistence.Relational.EfCore;

public sealed class SqliteReadModelSchemaInitializer<TContext> : IReadModelSchemaInitializer<TContext>
    where TContext : ReadModelContext
{
    private readonly ILogger<SqliteReadModelSchemaInitializer<TContext>> _logger;

    public SqliteReadModelSchemaInitializer(ILogger<SqliteReadModelSchemaInitializer<TContext>> logger)
    {
        _logger = logger;
    }

    public async Task InitializeAsync(TContext context, CancellationToken cancellationToken = default)
    {
        var database = context.Database;
        var connection = database.GetDbConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var relationalModel = context.Model.GetRelationalModel();
            var existingTables = await GetExistingTablesAsync(connection, cancellationToken).ConfigureAwait(false);

            foreach (var table in relationalModel.Tables)
            {
                var tableName = table.Name;
                if (!existingTables.Contains(tableName))
                {
                    await CreateTableAsync(connection, context, table, cancellationToken).ConfigureAwait(false);
                    existingTables.Add(tableName);
                    continue;
                }

                await EnsureColumnsAsync(connection, context, table, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            await connection.CloseAsync().ConfigureAwait(false);
        }
    }

    private static async Task<HashSet<string>> GetExistingTablesAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(reader.GetString(0));
        }
        return results;
    }

    private async Task<IDictionary<string, bool>> GetExistingColumnsAsync(DbConnection connection, string table, CancellationToken cancellationToken)
    {
        var columns = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info('{table}')";
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var name = reader.GetString(1);
            var notNull = reader.GetInt32(3) == 1;
            columns[name] = !notNull;
        }
        return columns;
    }

    private async Task CreateTableAsync(DbConnection connection, TContext context, ITable table, CancellationToken cancellationToken)
    {
        var sqlHelper = context.GetService<ISqlGenerationHelper>();
        var tableName = sqlHelper.DelimitIdentifier(table.Name);
        var columnSql = table.Columns.Select(c => BuildColumnDefinition(c, sqlHelper));
        var primaryKey = table.PrimaryKey is not null
            ? $"CONSTRAINT {sqlHelper.DelimitIdentifier(table.PrimaryKey.Name ?? $"PK_{table.Name}")} PRIMARY KEY ({string.Join(", ", table.PrimaryKey.Columns.Select(c => sqlHelper.DelimitIdentifier(c.Name)))})"
            : null;
        var components = primaryKey is null ? columnSql : columnSql.Concat(new[] { primaryKey });
        var commandText = $"CREATE TABLE {tableName} ({string.Join(", ", components)});";

        _logger.LogInformation("Creating SQLite read model table {TableName}", tableName);
        await ExecuteNonQueryAsync(connection, commandText, cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureColumnsAsync(DbConnection connection, TContext context, ITable table, CancellationToken cancellationToken)
    {
        var existingColumns = await GetExistingColumnsAsync(connection, table.Name, cancellationToken).ConfigureAwait(false);
        var sqlHelper = context.GetService<ISqlGenerationHelper>();
        var tableName = sqlHelper.DelimitIdentifier(table.Name);

        var missingRequired = table.Columns
            .Where(c => !existingColumns.ContainsKey(c.Name) && !c.IsNullable)
            .Select(c => c.Name)
            .ToArray();

        if (missingRequired.Length > 0)
        {
            _logger.LogWarning("Recreating SQLite read model table {Table} to add required columns: {Columns}", tableName, string.Join(", ", missingRequired));
            await RecreateTableAsync(connection, context, table, cancellationToken).ConfigureAwait(false);
            return;
        }

        foreach (var column in table.Columns)
        {
            if (existingColumns.ContainsKey(column.Name))
            {
                continue;
            }

            var columnDefinition = BuildColumnDefinition(column, sqlHelper);
            var commandText = $"ALTER TABLE {tableName} ADD COLUMN {columnDefinition};";
            _logger.LogInformation("Adding column {Column} to SQLite read model table {Table}", column.Name, tableName);
            await ExecuteNonQueryAsync(connection, commandText, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task RecreateTableAsync(DbConnection connection, TContext context, ITable table, CancellationToken cancellationToken)
    {
        var sqlHelper = context.GetService<ISqlGenerationHelper>();
        var tableName = sqlHelper.DelimitIdentifier(table.Name);

        var dropCommand = $"DROP TABLE IF EXISTS {tableName};";
        await ExecuteNonQueryAsync(connection, dropCommand, cancellationToken).ConfigureAwait(false);
        await CreateTableAsync(connection, context, table, cancellationToken).ConfigureAwait(false);
    }

    private static string BuildColumnDefinition(IColumnBase column, ISqlGenerationHelper sqlHelper)
    {
        var pieces = new List<string>
        {
            sqlHelper.DelimitIdentifier(column.Name),
            column.StoreType
        };

        pieces.Add(column.IsNullable ? "NULL" : "NOT NULL");
        return string.Join(" ", pieces);
    }

    private static async Task ExecuteNonQueryAsync(DbConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}


