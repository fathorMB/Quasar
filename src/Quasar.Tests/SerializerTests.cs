using Quasar.EventSourcing.Abstractions;
using Xunit;

namespace Quasar.Tests;

public class SerializerTests
{
    private sealed record Ev(int V) : IEvent;

    [Fact]
    public void SystemTextJson_serializer_roundtrips()
    {
        IEventTypeMap map = new DictionaryEventTypeMap(new[] { ("ev", typeof(Ev)) });
        var ser = new SystemTextJsonEventSerializer(map);
        var e = new Ev(42);
        var json = ser.Serialize(e, out var type);
        Assert.Equal("ev", type);
        var back = (Ev)ser.Deserialize(json, type);
        Assert.Equal(42, back.V);
    }
}

