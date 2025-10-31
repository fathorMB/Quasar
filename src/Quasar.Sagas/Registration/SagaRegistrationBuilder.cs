using Quasar.Sagas.Core;

namespace Quasar.Sagas.Registration;

public sealed class SagaRegistrationBuilder<TSaga, TState>
    where TSaga : class
    where TState : class, ISagaState, new()
{
    private readonly List<SagaMessageHandlerDescriptor> _descriptors = new();

    internal IReadOnlyCollection<SagaMessageHandlerDescriptor> Descriptors => _descriptors;

    public SagaRegistrationBuilder<TSaga, TState> StartsWith<TMessage>(Func<TMessage, Guid> correlation)
        where TMessage : notnull
    {
        var descriptor = SagaMessageHandlerDescriptor.Create<TSaga, TState, TMessage>(isStarter: true, correlation);
        _descriptors.Add(descriptor);
        return this;
    }

    public SagaRegistrationBuilder<TSaga, TState> Handles<TMessage>(Func<TMessage, Guid> correlation)
        where TMessage : notnull
    {
        var descriptor = SagaMessageHandlerDescriptor.Create<TSaga, TState, TMessage>(isStarter: false, correlation);
        _descriptors.Add(descriptor);
        return this;
    }
}
