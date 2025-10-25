using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace Quasar.Scheduling.Quartz;

internal sealed class ServiceProviderJobFactory : IJobFactory
{
    private readonly IServiceProvider _services;
    private readonly ConcurrentDictionary<IJob, IServiceScope> _scopes = new();

    public ServiceProviderJobFactory(IServiceProvider services)
    {
        _services = services;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var scope = _services.CreateScope();
        try
        {
            var job = (IJob)scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType);
            _scopes[job] = scope;
            return job;
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }

    public void ReturnJob(IJob job)
    {
        if (_scopes.TryRemove(job, out var scope))
        {
            scope.Dispose();
            return;
        }

        if (job is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
