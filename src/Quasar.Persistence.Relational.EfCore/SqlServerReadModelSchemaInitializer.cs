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

public sealed class SqlServerReadModelSchemaInitializer<TContext> : IReadModelSchemaInitializer<TContext>
    where TContext : ReadModelContext
{
    private readonly ILogger<SqlServerReadModelSchemaInitializer<TContext>> _logger;

    public SqlServerReadModelSchemaInitializer(ILogger<SqlServerReadModelSchemaInitializer<TContext>> logger)
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
                var schema = table.Schema ?? "dbo";
                var tableKey = CreateTableKey(schema, table.Name);
                if (!existingTables.Contains(tableKey))
                {
                    await CreateTableAsync(connection, context, table, schema, cancellationToken).ConfigureAwait(false);
                    existingTables.Add(tableKey);
                    continue;
                }

                await EnsureColumnsAsync(connection, context, table, schema, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            await connection.CloseAsync().ConfigureAwait(false);
        }
    }

    private static string CreateTableKey(string schema, string table) => $"{schema}:{table}";

    private static async Task<HashSet<string>> GetExistingTablesAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var schema = reader.GetString(0);
            var table = reader.GetString(1);
            results.Add(CreateTableKey(schema, table));
        }
        return results;
    }

    private static async Task<IDictionary<string, string>> GetExistingColumnsAsync(DbConnection connection, string schema, string table, CancellationToken cancellationToken)
    {
        var columns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table";
        var schemaParam = cmd.CreateParameter(); schemaParam.ParameterName = "@schema"; schemaParam.Value = schema; cmd.Parameters.Add(schemaParam);
        var tableParam = cmd.CreateParameter(); tableParam.ParameterName = "@table"; tableParam.Value = table; cmd.Parameters.Add(tableParam);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var name = reader.GetString(0);
            var type = reader.GetString(1);
            columns[name] = type;
        }
        return columns;
    }

    private async Task CreateTableAsync(DbConnection connection, TContext context, ITable table, string schema, CancellationToken cancellationToken)
    {
        var sqlHelper = context.GetService<ISqlGenerationHelper>();
        var tableName = sqlHelper.DelimitIdentifier(table.Name, schema);
        var columnSql = table.Columns.Select(c => BuildColumnDefinition(c, sqlHelper));
        var primaryKey = table.PrimaryKey is not null
            ? $"CONSTRAINT {sqlHelper.DelimitIdentifier(table.PrimaryKey.Name ?? $"PK_{table.Name}")} PRIMARY KEY ({string.Join(", ", table.PrimaryKey.Columns.Select(c => sqlHelper.DelimitIdentifier(c.Name)))})"
            : null;
        var components = primaryKey is null ? columnSql : columnSql.Concat(new[] { primaryKey });
        var commandText = $"CREATE TABLE {tableName} ({string.Join(", ", components)});";

        _logger.LogInformation("Creating read model table {TableName}", tableName);
        await ExecuteNonQueryAsync(connection, commandText, cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureColumnsAsync(DbConnection connection, TContext context, ITable table, string schema, CancellationToken cancellationToken)
    {
        var existingColumns = await GetExistingColumnsAsync(connection, schema, table.Name, cancellationToken).ConfigureAwait(false);
        var sqlHelper = context.GetService<ISqlGenerationHelper>();
        var tableName = sqlHelper.DelimitIdentifier(table.Name, schema);

        foreach (var column in table.Columns)
        {
            if (existingColumns.ContainsKey(column.Name))
            {
                continue;
            }

            var columnDefinition = BuildColumnDefinition(column, sqlHelper);
            var commandText = $"ALTER TABLE {tableName} ADD {columnDefinition};";
            _logger.LogInformation("Adding column {Column} to read model table {Table}", column.Name, tableName);
            await ExecuteNonQueryAsync(connection, commandText, cancellationToken).ConfigureAwait(false);
        }
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


