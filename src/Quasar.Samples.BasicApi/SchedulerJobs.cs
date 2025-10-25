using Quasar.Cqrs;
using Quasar.Scheduling.Quartz;

namespace Quasar.Samples.BasicApi;

/// <summary>
/// Demonstrates executing framework commands from a Quartz job.
/// </summary>
public sealed class IncrementCounterJob : IQuasarJob
{
    private readonly IMediator _mediator;

    public IncrementCounterJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task ExecuteAsync(QuasarJobContext context, CancellationToken cancellationToken)
    {
        await _mediator.Send(new IncrementCounterCommand(SampleConfig.DemoUserId, 1), cancellationToken);
    }
}
