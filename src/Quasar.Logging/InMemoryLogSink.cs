using Serilog.Core;
using Serilog.Events;

namespace Quasar.Logging;

internal sealed class InMemoryLogSink : ILogEventSink
{
    private readonly InMemoryLogBuffer _buffer;

    public InMemoryLogSink(InMemoryLogBuffer buffer)
    {
        _buffer = buffer;
    }

    public void Emit(LogEvent logEvent)
    {
        _buffer.Store(logEvent);
    }
}
