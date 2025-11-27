using System;
using Quasar.EventSourcing.Outbox.EfCore;
using Xunit;

namespace Quasar.Tests;

public class OutboxEntityTests
{
    [Fact]
    public void InboxMessageEntity_properties_work()
    {
        var entity = new InboxMessageEntity();
        var now = DateTime.UtcNow;
        var processed = now.AddMinutes(1);

        entity.Source = "source";
        entity.MessageId = "msg-1";
        entity.ReceivedUtc = now;
        entity.Hash = "hash";
        entity.ProcessedUtc = processed;

        Assert.Equal("source", entity.Source);
        Assert.Equal("msg-1", entity.MessageId);
        Assert.Equal(now, entity.ReceivedUtc);
        Assert.Equal("hash", entity.Hash);
        Assert.Equal(processed, entity.ProcessedUtc);
    }

    [Fact]
    public void OutboxMessageEntity_properties_work()
    {
        var entity = new OutboxMessageEntity();
        var id = Guid.NewGuid();
        var streamId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        entity.MessageId = id;
        entity.StreamId = streamId;
        entity.StreamVersion = 1;
        entity.EventName = "event";
        entity.Payload = "{}";
        entity.CreatedUtc = now;
        entity.DispatchedUtc = now.AddMinutes(1);
        entity.AttemptCount = 2;
        entity.LastAttemptUtc = now.AddMinutes(2);
        entity.LastError = "error";
        entity.Destination = "dest";
        entity.MetadataJson = "{}";

        Assert.Equal(id, entity.MessageId);
        Assert.Equal(streamId, entity.StreamId);
        Assert.Equal(1, entity.StreamVersion);
        Assert.Equal("event", entity.EventName);
        Assert.Equal("{}", entity.Payload);
        Assert.Equal(now, entity.CreatedUtc);
        Assert.Equal(now.AddMinutes(1), entity.DispatchedUtc);
        Assert.Equal(2, entity.AttemptCount);
        Assert.Equal(now.AddMinutes(2), entity.LastAttemptUtc);
        Assert.Equal("error", entity.LastError);
        Assert.Equal("dest", entity.Destination);
        Assert.Equal("{}", entity.MetadataJson);
    }
}
