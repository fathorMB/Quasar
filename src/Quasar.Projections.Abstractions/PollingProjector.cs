using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quasar.EventSourcing.Abstractions;

namespace Quasar.Projections.Abstractions;

public sealed class PollingProjector : IHostedService
{
    private readonly IServiceProvider _provider; // resolve scoped projections per dispatch
    private readonly string _projectorName;
    private readonly IReadOnlyList<Guid> _streamIds;
    private readonly TimeSpan _interval;
    private CancellationTokenSource? _cts;
    private Task? _runner;

    public PollingProjector(IServiceProvider provider,
        string projectorName, IEnumerable<Guid> streamIds, TimeSpan? pollingInterval = null)
    {
        _provider = provider;
        _projectorName = projectorName;
        _streamIds = streamIds.ToList();
        _interval = pollingInterval ?? TimeSpan.FromMilliseconds(500);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _runner = Task.Run(() => RunAsync(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is not null)
        {
            _cts.Cancel();
            try { if (_runner is not null) await _runner.ConfigureAwait(false); } catch { /* ignored */ }
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        // Flatten events across configured streams in version order per stream
        using var initScope = _provider.CreateScope();
        var checkpointStore = initScope.ServiceProvider.GetRequiredService<ICheckpointStore>();
        var store = initScope.ServiceProvider.GetRequiredService<IEventStore>();

        var perStreamVersion = new Dictionary<Guid, int>();
        foreach (var sid in _streamIds)
        {
            var key = StreamKey(sid);
            var lastForStream = await checkpointStore.GetCheckpointAsync(key, ct).ConfigureAwait(false);
            perStreamVersion[sid] = (int)lastForStream;
        }

        while (!ct.IsCancellationRequested)
        {
            var any = false;
            foreach (var sid in _streamIds)
            {
                var from = perStreamVersion[sid];
                var batch = await store.ReadStreamAsync(sid, from, ct).ConfigureAwait(false);
                if (batch.Count == 0) continue;
                any = true;
                foreach (var env in batch)
                {
                    await DispatchAsync(env, ct).ConfigureAwait(false);
                    perStreamVersion[sid] = env.Version;
                    await checkpointStore.SaveCheckpointAsync(StreamKey(sid), env.Version, ct).ConfigureAwait(false);
                }
            }

            if (!any)
                await Task.Delay(_interval, ct).ConfigureAwait(false);
        }
    }

    private string StreamKey(Guid streamId) => $"{_projectorName}:{streamId}";

    private Task DispatchAsync(EventEnvelope envelope, CancellationToken ct)
    {
        var evtType = envelope.Event.GetType();
        using var scope = _provider.CreateScope();
        var projections = scope.ServiceProvider.GetServices<object>();
        foreach (var proj in projections)
        {
            var pt = proj.GetType();
            foreach (var iface in pt.GetInterfaces())
            {
                if (!iface.IsGenericType) continue;
                var def = iface.GetGenericTypeDefinition();
                if (def != typeof(IProjection<>)) continue;
                var arg = iface.GetGenericArguments()[0];
                if (!arg.IsAssignableFrom(evtType)) continue;
                var method = pt.GetMethods()
                    .Where(m => m.Name == "HandleAsync")
                    .FirstOrDefault(m =>
                    {
                        var ps = m.GetParameters();
                        return ps.Length == 2 && ps[0].ParameterType.IsAssignableFrom(evtType) && ps[1].ParameterType == typeof(CancellationToken);
                    });
                if (method is null) continue;
                return (Task)method.Invoke(proj, new object?[] { envelope.Event, ct })!;
            }
        }
        return Task.CompletedTask;
    }
}



