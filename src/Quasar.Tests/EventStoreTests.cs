using Quasar.EventSourcing.Abstractions;
using Quasar.EventSourcing.InMemory;
using Xunit;

namespace Quasar.Tests;

public class EventStoreTests
{
    private sealed record SomethingHappened(int X) : IEvent;

    [Fact]
    public async Task Append_and_read_back()
    {
        IEventStore store = new InMemoryEventStore();
        var id = Guid.NewGuid();
        await store.AppendAsync(id, 0, new IEvent[] { new SomethingHappened(1), new SomethingHappened(2) });
        var events = await store.ReadStreamAsync(id);
        Assert.Equal(2, events.Count);
        Assert.Equal(1, ((SomethingHappened)events[0].Event).X);
        Assert.Equal(2, ((SomethingHappened)events[1].Event).X);
    }

    [Fact]
    public async Task Wrong_expected_version_throws()
    {
        IEventStore store = new InMemoryEventStore();
        var id = Guid.NewGuid();
        await store.AppendAsync(id, 0, new IEvent[] { new SomethingHappened(1) });
        await Assert.ThrowsAsync<ConcurrencyException>(() => store.AppendAsync(id, 0, new IEvent[] { new SomethingHappened(2) }));
    }
}

