using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quasar.Sagas.Core;
using Quasar.Sagas.Persistence;

namespace Quasar.Sagas.Registration;

internal sealed class SagaRegistrationStore
{
    private readonly List<SagaMessageHandlerDescriptor> _descriptors = new();

    public IReadOnlyCollection<SagaMessageHandlerDescriptor> Descriptors
    {
        get
        {
            lock (_descriptors)
            {
                return _descriptors.ToArray();
            }
        }
    }

    public void AddDescriptors(IEnumerable<SagaMessageHandlerDescriptor> descriptors)
    {
        lock (_descriptors)
        {
            _descriptors.AddRange(descriptors);
        }
    }
}

public interface ISagaConfigurator
{
    ISagaConfigurator AddSaga<TSaga, TState>(Action<SagaRegistrationBuilder<TSaga, TState>> configure)
        where TSaga : class
        where TState : class, ISagaState, new();
}

internal sealed class SagaConfigurator : ISagaConfigurator
{
    private readonly IServiceCollection _services;
    private readonly SagaRegistrationStore _store;

    public SagaConfigurator(IServiceCollection services, SagaRegistrationStore store)
    {
        _services = services;
        _store = store;
    }

    public ISagaConfigurator AddSaga<TSaga, TState>(Action<SagaRegistrationBuilder<TSaga, TState>> configure)
        where TSaga : class
        where TState : class, ISagaState, new()
    {
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        _services.TryAddScoped<TSaga>();
        _services.TryAddSingleton(typeof(ISagaRepository<TState>), typeof(InMemorySagaRepository<TState>));

        var builder = new SagaRegistrationBuilder<TSaga, TState>();
        configure(builder);

        _store.AddDescriptors(builder.Descriptors);
        return this;
    }
}
