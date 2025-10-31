using Microsoft.Extensions.Logging;
using Quasar.Cqrs;
using Quasar.Sagas.Core;

namespace Quasar.Sagas;

/// <summary>
/// Mediator pipeline behavior that routes commands to registered sagas after successful handler execution.
/// </summary>
/// <typeparam name="TRequest">Request type.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
internal sealed class SagaPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private static readonly bool IsCommand = typeof(ICommand<TResponse>).IsAssignableFrom(typeof(TRequest));

    private readonly ISagaCoordinator _coordinator;
    private readonly ILogger<SagaPipelineBehavior<TRequest, TResponse>> _logger;

    public SagaPipelineBehavior(
        ISagaCoordinator coordinator,
        ILogger<SagaPipelineBehavior<TRequest, TResponse>> logger)
    {
        _coordinator = coordinator;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
    {
        if (next is null) throw new ArgumentNullException(nameof(next));

        var response = await next().ConfigureAwait(false);

        if (IsCommand && request is not null)
        {
            try
            {
                await _coordinator.ProcessAsync(request!, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saga processing failed for {RequestType}.", request.GetType().Name);
                throw;
            }
        }

        return response;
    }
}
