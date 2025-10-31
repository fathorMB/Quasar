using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quasar.Sagas.Persistence;

namespace Quasar.Sagas.Persistence.Relational.EfCore;

public static class SagaRelationalPersistenceBuilderExtensions
{
    public static ISagaPersistenceBuilder UseSagaDbContext(
        this ISagaPersistenceBuilder builder,
        Action<IServiceProvider, DbContextOptionsBuilder> configure)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        builder.Services.AddDbContext<SagaDbContext>(configure);
        builder.UseRepository(typeof(EfSagaRepository<>));
        return builder;
    }

    public static ISagaPersistenceBuilder UseSagaDbContext(
        this ISagaPersistenceBuilder builder,
        Action<DbContextOptionsBuilder> configure)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        return builder.UseSagaDbContext((_, options) => configure(options));
    }

    public static ISagaPersistenceBuilder UseSqlServerSagaStore(
        this ISagaPersistenceBuilder builder,
        string connectionString,
        Action<SqlServerSagaOptions>? configure = null)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Connection string is required.", nameof(connectionString));

        builder.UseSagaDbContext((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(SagaDbContext).Assembly.FullName);
            });

            configure?.Invoke(new SqlServerSagaOptions(options));
        });

        return builder;
    }

    public static ISagaPersistenceBuilder UseSqliteSagaStore(
        this ISagaPersistenceBuilder builder,
        string connectionString,
        Action<SqliteSagaOptions>? configure = null)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Connection string is required.", nameof(connectionString));

        builder.UseSagaDbContext((sp, options) =>
        {
            options.UseSqlite(connectionString);
            configure?.Invoke(new SqliteSagaOptions(options));
        });

        return builder;
    }
}

public readonly record struct SqlServerSagaOptions(DbContextOptionsBuilder Builder);
public readonly record struct SqliteSagaOptions(DbContextOptionsBuilder Builder);
