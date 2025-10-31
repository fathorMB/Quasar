using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quasar.Cqrs;
using Quasar.Sagas.Core;
using Quasar.Sagas.Persistence;
using Quasar.Sagas.Registration;

namespace Quasar.Sagas;

public static class SagaServiceCollectionExtensions
{
    public static IServiceCollection AddQuasarSagas(
        this IServiceCollection services,
        Action<ISagaConfigurator>? configure = null,
        Action<ISagaPersistenceBuilder>? configurePersistence = null)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        var storeDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(SagaRegistrationStore));
        SagaRegistrationStore store;
        if (storeDescriptor?.ImplementationInstance is SagaRegistrationStore existingStore)
        {
            store = existingStore;
        }
        else
        {
            store = new SagaRegistrationStore();
            services.AddSingleton(store);
        }

        services.TryAddSingleton<ISagaRegistry>(sp =>
        {
            var registry = new SagaRegistry();
            var registrationStore = sp.GetRequiredService<SagaRegistrationStore>();
            registry.Register(registrationStore.Descriptors);
            return registry;
        });

        services.TryAddScoped<ISagaCoordinator, SagaCoordinator>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(SagaPipelineBehavior<,>));

        if (configure is not null)
        {
            var configurator = new SagaConfigurator(services, store);
            configure(configurator);
        }

        var persistenceBuilder = new SagaPersistenceBuilder(services);
        configurePersistence?.Invoke(persistenceBuilder);
        persistenceBuilder.Build();

        return services;
    }
}
