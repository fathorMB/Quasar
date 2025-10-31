using System.Collections.Concurrent;

namespace Quasar.Sagas.Core;

internal interface ISagaRegistry
{
    void Register(IEnumerable<SagaMessageHandlerDescriptor> descriptors);
    IReadOnlyList<SagaMessageHandlerDescriptor> Resolve(Type messageType);
}

internal sealed class SagaRegistry : ISagaRegistry
{
    private readonly ConcurrentDictionary<Type, List<SagaMessageHandlerDescriptor>> _handlers = new();

    public void Register(IEnumerable<SagaMessageHandlerDescriptor> descriptors)
    {
        foreach (var descriptor in descriptors)
        {
            var list = _handlers.GetOrAdd(descriptor.MessageType, _ => new List<SagaMessageHandlerDescriptor>());
            lock (list)
            {
                list.Add(descriptor);
            }
        }
    }

    public IReadOnlyList<SagaMessageHandlerDescriptor> Resolve(Type messageType)
    {
        if (messageType is null) throw new ArgumentNullException(nameof(messageType));
        if (_handlers.TryGetValue(messageType, out var direct))
        {
            return direct;
        }
        return Array.Empty<SagaMessageHandlerDescriptor>();
    }
}
