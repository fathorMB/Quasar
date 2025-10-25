using Quartz;
using Quasar.Cqrs;

namespace Quasar.Samples.BasicApi;

/// <summary>
/// Demonstrates executing framework commands from a Quartz job.
/// </summary>
public sealed class IncrementCounterJob : IJob
{
    private readonly IMediator _mediator;

    public IncrementCounterJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _mediator.Send(new IncrementCounterCommand(SampleConfig.DemoUserId, 1), context.CancellationToken);
    }
}
