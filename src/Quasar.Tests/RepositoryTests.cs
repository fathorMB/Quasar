using Quasar.Domain;
using Quasar.EventSourcing.Abstractions;
using Quasar.EventSourcing.InMemory;
using Xunit;

namespace Quasar.Tests;

public class RepositoryTests
{
    private sealed record Incremented(int Amount) : IEvent;

    private sealed class Counter : AggregateRoot
    {
        public int Sum { get; private set; }
        public Counter() { Id = Guid.NewGuid(); }
        public void Inc(int x) => ApplyChange(new Incremented(x));
        private void When(Incremented i)
        {
            Sum += i.Amount;
        }
    }

    [Fact]
    public async Task Save_and_load_event_sourced_aggregate()
    {
        IEventStore store = new InMemoryEventStore();
        var repo = new EventSourcedRepository<Counter>(store);
        var c = new Counter();
        var id = c.Id;
        c.Inc(2);
        c.Inc(3);
        await repo.SaveAsync(c);

        var re = await repo.GetAsync(id);
        Assert.Equal(5, re.Sum);
        Assert.Equal(2, re.Version);
    }
}

