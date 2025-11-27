using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Quasar.Core;
using Quasar.Domain;
using Quasar.EventSourcing.Abstractions;
using Quasar.EventSourcing.Outbox;
using Quasar.EventSourcing.Outbox.EfCore;

namespace Quasar.Tests;

public class OutboxTests
{
    [Fact]
    public async Task RepositoryEnqueuesMessagesWhenOutboxConfigured()
    {
        var eventStore = new InMemoryEventStore();
        var outboxStore = new CapturingOutboxStore();
        var factory = new PassthroughOutboxMessageFactory();
        var repository = new EventSourcedRepository<FakeAggregate>(eventStore, outboxStore, factory);

        var aggregate = new FakeAggregate();
        aggregate.RaiseTestEvent();

        await repository.SaveAsync(aggregate);

        Assert.Single(outboxStore.Messages);
        var message = outboxStore.Messages[0];
        Assert.Equal(FakeAggregate.StreamId, message.StreamId);
        Assert.Equal(1, message.StreamVersion);
        Assert.IsType<FakeEvent>(message.Event);
    }

    [Fact]
    public async Task EfCoreOutboxStorePersistsMessages()
    {
        var options = new DbContextOptionsBuilder<OutboxDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new OutboxDbContext(options);
        var store = new EfCoreOutboxStore(context);

        var message = new OutboxMessage(
            Guid.NewGuid(),
            FakeAggregate.StreamId,
            1,
            new FakeEvent(Guid.NewGuid()),
            DateTime.UtcNow,
            "fake.event",
            "{}");

        await store.EnqueueAsync(new[] { message });

        var persisted = await context.OutboxMessages.SingleAsync();
        Assert.Equal(message.MessageId, persisted.MessageId);
        Assert.Equal("fake.event", persisted.EventName);
    }

    [Fact]
    public async Task EfCoreOutboxStorePersistsAndDeserializesMetadata()
    {
        var options = new DbContextOptionsBuilder<OutboxDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new OutboxDbContext(options);
        var store = new EfCoreOutboxStore(context);

        var metadata = new Dictionary<string, string> { { "key", "value" } };
        var message = new OutboxMessage(
            Guid.NewGuid(),
            FakeAggregate.StreamId,
            1,
            new FakeEvent(Guid.NewGuid()),
            DateTime.UtcNow,
            "fake.event",
            "{}",
            Metadata: metadata);

        await store.EnqueueAsync(new[] { message });

        var pending = await store.GetPendingAsync(10, 5);
        
        Assert.Single(pending);
        var loaded = pending[0];
        Assert.NotNull(loaded.Metadata);
        Assert.Equal("value", loaded.Metadata!["key"]);
    }

    [Fact]
    public async Task EfCoreInboxStoreRejectsDuplicates()
    {
        var options = new DbContextOptionsBuilder<OutboxDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new OutboxDbContext(options);
        var store = new EfCoreInboxStore(context);

        var incoming = new InboxMessage("tests", "msg-1", DateTime.UtcNow);

        var first = await store.TryEnsureProcessedAsync(incoming);
        var second = await store.TryEnsureProcessedAsync(incoming);

        Assert.True(first);
        Assert.False(second);

        var count = await context.InboxMessages.CountAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task EfCoreOutboxStoreHonorsMaxAttemptsFilter()
    {
        var options = new DbContextOptionsBuilder<OutboxDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new OutboxDbContext(options);
        var store = new EfCoreOutboxStore(context);

        var message = new OutboxMessage(
            Guid.NewGuid(),
            FakeAggregate.StreamId,
            1,
            new FakeEvent(Guid.NewGuid()),
            DateTime.UtcNow,
            "fake.event",
            "{}");

        await store.EnqueueAsync(new[] { message });
        await store.RecordDispatchOutcomeAsync(message.MessageId, DateTime.UtcNow, succeeded: false, error: "failed");

        var pending = await store.GetPendingAsync(batchSize: 10, maxAttempts: 1);

        Assert.Empty(pending);
    }

    [Fact]
    public async Task OutboxDispatcherPublishesWithRegisteredPublisher()
    {
        var store = new DispatcherOutboxStore();
        var publisher = new TestOutboxPublisher("kafka");
        var options = Options.Create(new OutboxDispatcherOptions
        {
            BatchSize = 10,
            PollInterval = TimeSpan.FromMilliseconds(50),
            MaxAttempts = 3,
            DefaultPublisherName = "kafka"
        });

        var dispatcher = new OutboxDispatcher(
            store,
            new[] { publisher },
            options,
            new SystemClock(),
            NullLogger<OutboxDispatcher>.Instance);

        await dispatcher.StartAsync(CancellationToken.None);
        await Task.Delay(200);
        await dispatcher.StopAsync(CancellationToken.None);

        Assert.True(publisher.Published.Count == 1);
        Assert.Contains(store.Outcomes, outcome => outcome.success);
    }

    private sealed class PassthroughOutboxMessageFactory : IOutboxMessageFactory
    {
        public IReadOnlyList<OutboxMessage> Create(Guid streamId, int startingVersion, IReadOnlyList<IDomainEvent> events)
        {
            var created = new List<OutboxMessage>(events.Count);
            for (var i = 0; i < events.Count; i++)
            {
                if (events[i] is not FakeEvent fakeEvent)
                {
                    continue;
                }

                created.Add(new OutboxMessage(
                    Guid.NewGuid(),
                    streamId,
                    startingVersion + i + 1,
                    fakeEvent,
                    DateTime.UtcNow,
                    "fake.event",
                    "{}"));
            }

            return created;
        }
    }

    private sealed class CapturingOutboxStore : IOutboxStore
    {
        public List<OutboxMessage> Messages { get; } = new();

        public Task EnqueueAsync(IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken = default)
        {
            Messages.AddRange(messages);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<OutboxPendingMessage>> GetPendingAsync(int batchSize, int maxAttempts, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<OutboxPendingMessage>>(Array.Empty<OutboxPendingMessage>());

        public Task RecordDispatchOutcomeAsync(Guid messageId, DateTime attemptUtc, bool succeeded, string? error = null, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class DispatcherOutboxStore : IOutboxStore
    {
        private bool _dequeued;
        private readonly OutboxPendingMessage _message = new(
            Guid.NewGuid(),
            FakeAggregate.StreamId,
            1,
            "fake.event",
            "{}",
            DateTime.UtcNow,
            0,
            null,
            "kafka",
            null,
            null);

        public List<(Guid messageId, bool success)> Outcomes { get; } = new();

        public Task EnqueueAsync(IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<OutboxPendingMessage>> GetPendingAsync(int batchSize, int maxAttempts, CancellationToken cancellationToken = default)
        {
            if (_dequeued)
            {
                return Task.FromResult<IReadOnlyList<OutboxPendingMessage>>(Array.Empty<OutboxPendingMessage>());
            }

            _dequeued = true;
            return Task.FromResult<IReadOnlyList<OutboxPendingMessage>>(new[] { _message });
        }

        public Task RecordDispatchOutcomeAsync(Guid messageId, DateTime attemptUtc, bool succeeded, string? error = null, CancellationToken cancellationToken = default)
        {
            Outcomes.Add((messageId, succeeded));
            return Task.CompletedTask;
        }
    }

    private sealed class TestOutboxPublisher : IOutboxPublisher
    {
        public TestOutboxPublisher(string name) => Name = name;

        public string Name { get; }

        public List<OutboxPendingMessage> Published { get; } = new();

        public Task PublishAsync(OutboxPendingMessage message, CancellationToken cancellationToken = default)
        {
            Published.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryEventStore : IEventStore
    {
        public List<IEvent> Appended { get; } = new();

        public Task AppendAsync(Guid streamId, int expectedVersion, IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
        {
            Appended.AddRange(events);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<EventEnvelope>> ReadStreamAsync(Guid streamId, int fromVersion = 0, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<EventEnvelope>>(Array.Empty<EventEnvelope>());
        }
    }

    private sealed class FakeAggregate : AggregateRoot
    {
        public static readonly Guid StreamId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public void RaiseTestEvent()
        {
            Id = StreamId;
            ApplyChange(new FakeEvent(Guid.NewGuid()));
        }

        private void When(FakeEvent @event)
        {
            // no state
        }
    }

    private sealed record FakeEvent(Guid Id) : IEvent;
}

