using System.Text.Json;

namespace Quasar.EventSourcing.Abstractions;

public interface IEventTypeMap
{
    Type Resolve(string typeName);
    string GetName(Type eventType);
}

public sealed class DictionaryEventTypeMap : IEventTypeMap
{
    private readonly IReadOnlyDictionary<string, Type> _nameToType;
    private readonly IReadOnlyDictionary<Type, string> _typeToName;

    public DictionaryEventTypeMap(IEnumerable<(string Name, Type Type)> mappings)
    {
        var nameToType = new Dictionary<string, Type>(StringComparer.Ordinal);
        var typeToName = new Dictionary<Type, string>();
        foreach (var (name, type) in mappings)
        {
            nameToType[name] = type;
            typeToName[type] = name;
        }
        _nameToType = nameToType;
        _typeToName = typeToName;
    }

    public Type Resolve(string typeName)
        => _nameToType.TryGetValue(typeName, out var t)
            ? t
            : throw new InvalidOperationException($"Unknown event type '{typeName}'.");

    public string GetName(Type eventType)
        => _typeToName.TryGetValue(eventType, out var n)
            ? n
            : throw new InvalidOperationException($"Event type '{eventType.FullName}' is not mapped.");
}

public sealed class SystemTextJsonEventSerializer : IEventSerializer
{
    private readonly JsonSerializerOptions _options;
    private readonly IEventTypeMap _typeMap;

    public SystemTextJsonEventSerializer(IEventTypeMap typeMap, JsonSerializerOptions? options = null)
    {
        _typeMap = typeMap;
        _options = options ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public string Serialize(IEvent @event, out string type)
    {
        type = _typeMap.GetName(@event.GetType());
        return JsonSerializer.Serialize(@event, @event.GetType(), _options);
    }

    public IEvent Deserialize(string payload, string type)
    {
        var clr = _typeMap.Resolve(type);
        var obj = JsonSerializer.Deserialize(payload, clr, _options);
        return (IEvent)(obj ?? throw new InvalidOperationException($"Failed to deserialize event '{type}'."));
    }
}

