using Quasar.Domain;
using Quasar.EventSourcing.Abstractions;
using Quasar.EventSourcing.InMemory;

namespace Quasar.Tests;

public class EventSourcing_SampleTests
{
    private sealed record CounterIncremented(int Amount) : IEvent;

    private sealed class CounterAggregate : AggregateRoot
    {
        public int Count { get; private set; }

        public CounterAggregate() { Id = Guid.NewGuid(); }

        public void Increment(int amount)
        {
            ApplyChange(new CounterIncremented(amount));
        }

        private void When(CounterIncremented e)
        {
            Count += e.Amount;
        }
    }

    [Fact]
    public async Task Append_and_read_via_repository_works()
    {
        // Arrange
        IEventStore store = new InMemoryEventStore();
        var repo = new EventSourcedRepository<CounterAggregate>(store);
        var agg = new CounterAggregate();

        // Act: perform two increments and persist
        agg.Increment(2);
        agg.Increment(3);
        await repo.SaveAsync(agg);

        // Load again and verify state
        var loaded = await repo.GetAsync(agg.Id);

        // Assert
        Assert.Equal(5, loaded.Count);
        Assert.Equal(2, loaded.Version);
    }
}

