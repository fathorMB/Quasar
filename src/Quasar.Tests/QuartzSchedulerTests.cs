using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using Quasar.Scheduling.Quartz;
using System.Linq;
using Xunit;

namespace Quasar.Tests;

public class QuartzSchedulerTests
{
    [Fact]
    public async Task HostedService_registers_jobs()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddQuartzScheduler(options =>
        {
            options.SchedulerName = "TestScheduler";
            options.Configure = builder =>
            {
                builder.ScheduleJob<StubJob>(
                    job => job.WithIdentity("stub", "tests"),
                    trigger => trigger.WithIdentity("stub-trigger", "tests").StartNow());
            };
        });
        services.AddTransient<StubJob>();

        var provider = services.BuildServiceProvider();
        var hosted = provider.GetServices<IHostedService>().Single();

        await hosted.StartAsync(CancellationToken.None);
        try
        {
            var accessor = provider.GetRequiredService<IQuartzSchedulerAccessor>();
            Assert.NotNull(accessor.Scheduler);
            var jobKeys = await accessor.Scheduler!.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            Assert.Single(jobKeys);
            Assert.Equal("stub", jobKeys.First().Name);
        }
        finally
        {
            await hosted.StopAsync(CancellationToken.None);
        }
    }

    private class StubJob : IJob
    {
        public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
    }
}


