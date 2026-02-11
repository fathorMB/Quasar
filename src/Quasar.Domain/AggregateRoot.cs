using Quasar.Core;
using System.Collections.Concurrent;
using System.Reflection;

namespace Quasar.Domain;

/// <summary>
/// Marker interface that identifies domain events emitted by aggregates.
/// </summary>
public interface IDomainEvent { }

/// <summary>
/// Base type for aggregate roots that track domain events and support event sourcing.
/// </summary>
public abstract class AggregateRoot
{
    private static readonly ConcurrentDictionary<(Type AggregateType, Type EventType), MethodInfo?> WhenCache = new();
    
    private readonly List<IDomainEvent> _uncommitted = new();

    /// <summary>
    /// Gets the identifier of the aggregate.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Framework internal: Sets the aggregate ID.
    /// </summary>
    public void SetId(Guid id) => Id = id;

    /// <summary>
    /// Gets the current aggregate version derived from the number of applied events.
    /// </summary>
    public int Version { get; protected set; } = 0;

    /// <summary>
    /// Applies and records a new domain event to the aggregate.
    /// </summary>
    /// <param name="event">The event to apply.</param>
    protected void ApplyChange(IDomainEvent @event)
    {
        When(@event);
        _uncommitted.Add(@event);
        Version++;
    }

    /// <summary>
    /// Replays the provided event history to rebuild the aggregate state.
    /// </summary>
    /// <param name="history">Historical events in order of occurrence.</param>
    public void Rehydrate(IEnumerable<IDomainEvent> history)
    {
        foreach (var e in history)
        {
            When(e);
            Version++;
        }
    }

    /// <summary>
    /// Applies the effects of <paramref name="event"/> to the aggregate state by invoking the conventional "When" method.
    /// </summary>
    protected virtual void When(IDomainEvent @event)
    {
        var eventType = @event.GetType();

        var aggregateType = GetType();
        var when = WhenCache.GetOrAdd((aggregateType, eventType), key =>
            key.AggregateType.GetMethod("When", BindingFlags.Instance | BindingFlags.NonPublic, new[] { key.EventType })
        );

        when?.Invoke(this, new object[] { @event });
    }

    /// <summary>
    /// Dequeues all uncommitted domain events and clears the internal buffer.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DequeueUncommitted()
    {
        var events = _uncommitted.ToArray();
        _uncommitted.Clear();
        return events;
    }
}

/// <summary>
/// Base type for immutable value objects that implement structural equality.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Gets the components used to evaluate equality.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject other || other.GetType() != GetType())
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) => HashCode.Combine(current, obj));
    }
}
