namespace Quasar.Seeding;

public interface IDataSeed
{
    Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}

public interface IOrderedDataSeed : IDataSeed
{
    int Order { get; }
}
