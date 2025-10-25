using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Quasar.Scheduling.Quartz;

internal static class QuartzSchemaInitializer
{
    public static async Task EnsureSchemaAsync(NameValueCollection properties, ILogger logger, CancellationToken cancellationToken)
    {
        var jobStoreType = properties["quartz.jobStore.type"];
        if (string.IsNullOrWhiteSpace(jobStoreType) || jobStoreType.Contains("RAMJobStore", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var provider = properties["quartz.dataSource.default.provider"];
        var connectionString = properties["quartz.dataSource.default.connectionString"];
        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogDebug("Quartz scheduler configured with persistent store but provider/connection string not supplied; skipping schema initialization.");
            return;
        }

        try
        {
            switch (provider.Trim().ToLowerInvariant())
            {
                case "sqlserver":
                case "sqlserver-20":
                case "sqlserver20":
                    await EnsureSqlServerAsync(properties, connectionString, logger, cancellationToken).ConfigureAwait(false);
                    break;
                case "sqlite-microsoft":
                case "sqlite":
                    await EnsureSqliteAsync(properties, connectionString, logger, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    logger.LogDebug("Quartz scheduler schema initializer: provider '{Provider}' not supported, skipping automatic schema creation.", provider);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Quartz scheduler schema initialization failed for provider '{Provider}'. The scheduler may not start correctly until the schema is created.", provider);
        }
    }

    private static async Task EnsureSqlServerAsync(NameValueCollection properties, string connectionString, ILogger logger, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (await TableExistsAsync(connection, "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QRTZ_JOB_DETAILS'", cancellationToken).ConfigureAwait(false))
        {
            await EnsureLocksAsync(connection, properties, logger, cancellationToken).ConfigureAwait(false);
            return;
        }

        await ExecuteNonQueryAsync(connection, QuartzSqlScripts.SqlServer, cancellationToken).ConfigureAwait(false);
        await EnsureLocksAsync(connection, properties, logger, cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Initialized Quartz scheduler schema for SQL Server.");
    }

    private static async Task EnsureSqliteAsync(NameValueCollection properties, string connectionString, ILogger logger, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (await TableExistsAsync(connection, "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'QRTZ_JOB_DETAILS'", cancellationToken).ConfigureAwait(false))
        {
            await EnsureLocksAsync(connection, properties, logger, cancellationToken).ConfigureAwait(false);
            return;
        }

        await ExecuteNonQueryAsync(connection, QuartzSqlScripts.Sqlite, cancellationToken).ConfigureAwait(false);
        await EnsureLocksAsync(connection, properties, logger, cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Initialized Quartz scheduler schema for SQLite.");
    }

    private static async Task<bool> TableExistsAsync(DbConnection connection, string commandText, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(result, System.Globalization.CultureInfo.InvariantCulture) > 0;
    }

    private static async Task ExecuteNonQueryAsync(DbConnection connection, string commandText, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureLocksAsync(DbConnection connection, NameValueCollection properties, ILogger logger, CancellationToken cancellationToken)
    {
        var schedulerName = properties["quartz.scheduler.instanceName"] ?? "DEFAULT";
        var existingLocks = await GetExistingLocksAsync(connection, schedulerName, cancellationToken).ConfigureAwait(false);
        if (existingLocks.Count >= 5)
        {
            return;
        }

        var lockNames = new[]
        {
            "TRIGGER_ACCESS",
            "JOB_ACCESS",
            "CALENDAR_ACCESS",
            "STATE_ACCESS",
            "MISFIRE_ACCESS"
        };

        foreach (var lockName in lockNames)
        {
            if (existingLocks.Contains(lockName, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            await using var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = "INSERT INTO QRTZ_LOCKS (SCHED_NAME, LOCK_NAME) VALUES (@sched, @lock)";
            var schedParam = insertCommand.CreateParameter();
            schedParam.ParameterName = "@sched";
            schedParam.Value = schedulerName;
            insertCommand.Parameters.Add(schedParam);

            var lockParam = insertCommand.CreateParameter();
            lockParam.ParameterName = "@lock";
            lockParam.Value = lockName;
            insertCommand.Parameters.Add(lockParam);

            await insertCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        logger.LogDebug("Ensured Quartz lock rows for scheduler '{SchedulerName}'.", schedulerName);
    }

    private static async Task<HashSet<string>> GetExistingLocksAsync(DbConnection connection, string schedulerName, CancellationToken cancellationToken)
    {
        var locks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT LOCK_NAME FROM QRTZ_LOCKS WHERE SCHED_NAME = @sched";
        var param = command.CreateParameter();
        param.ParameterName = "@sched";
        param.Value = schedulerName;
        command.Parameters.Add(param);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            locks.Add(reader.GetString(0));
        }

        return locks;
    }
}
