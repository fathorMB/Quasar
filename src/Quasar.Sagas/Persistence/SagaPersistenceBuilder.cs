using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Quasar.Sagas.Persistence;

internal sealed class SagaPersistenceBuilder : ISagaPersistenceBuilder
{
    private readonly IServiceCollection _services;
    private Action<IServiceCollection>? _configure;

    public SagaPersistenceBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public IServiceCollection Services => _services;

    public void UseRepository(Type openGenericRepositoryType)
    {
        if (openGenericRepositoryType is null) throw new ArgumentNullException(nameof(openGenericRepositoryType));
        _configure = services =>
        {
            services.RemoveAll(typeof(ISagaRepository<>));
            if (openGenericRepositoryType == typeof(InMemorySagaRepository<>))
            {
                services.AddSingleton(typeof(ISagaRepository<>), openGenericRepositoryType);
            }
            else
            {
                services.AddScoped(typeof(ISagaRepository<>), openGenericRepositoryType);
            }
        };
    }

    public void Build()
    {
        if (_configure is null)
        {
            UseRepository(typeof(InMemorySagaRepository<>));
        }

        _configure!(_services);
    }
}
