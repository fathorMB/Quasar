using Quasar.Core;

namespace Quasar.Domain;

public interface IDomainEvent { }

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _uncommitted = new();

    public Guid Id { get; protected set; }
    public int Version { get; protected set; } = 0;

    protected void ApplyChange(IDomainEvent @event)
    {
        When(@event);
        _uncommitted.Add(@event);
        Version++;
    }

    public void Rehydrate(IEnumerable<IDomainEvent> history)
    {
        foreach (var e in history)
        {
            When(e);
            Version++;
        }
    }

    protected abstract void When(IDomainEvent @event);

    public IReadOnlyList<IDomainEvent> DequeueUncommitted()
    {
        var events = _uncommitted.ToArray();
        _uncommitted.Clear();
        return events;
    }
}

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject other || other.GetType() != GetType())
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) => HashCode.Combine(current, obj));
    }
}

