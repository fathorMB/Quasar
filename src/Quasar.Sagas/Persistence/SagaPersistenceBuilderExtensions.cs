using Microsoft.Extensions.DependencyInjection;

namespace Quasar.Sagas.Persistence;

public static class SagaPersistenceBuilderExtensions
{
    public static ISagaPersistenceBuilder UseInMemorySagaStore(this ISagaPersistenceBuilder builder)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        builder.UseRepository(typeof(InMemorySagaRepository<>));
        return builder;
    }
}
