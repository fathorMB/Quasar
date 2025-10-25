using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using Quasar.Cqrs;

namespace Quasar.Telemetry;

/// <summary>
/// A static class to hold the ActivitySource for the Quasar framework.
/// </summary>
public static class QuasarActivitySource
{
    /// <summary>
    /// The name of the activity source.
    /// </summary>
    public const string Name = "Quasar";

    /// <summary>
    /// The singleton instance of the ActivitySource.
    /// </summary>
    public static readonly ActivitySource Instance = new(Name);
}

/// <summary>
/// A mediator pipeline behavior that creates OpenTelemetry activities for requests.
/// </summary>
public sealed class TelemetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<TelemetryBehavior<TRequest, TResponse>> _logger;

    public TelemetryBehavior(ILogger<TelemetryBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
    {
        var requestName = typeof(TRequest).Name;

        using var activity = QuasarActivitySource.Instance.StartActivity(requestName, ActivityKind.Internal);

        _logger.LogTrace("Beginning activity {ActivityName} for request {RequestName}", requestName, requestName);
        activity?.SetTag("quasar.request_type", typeof(TRequest).FullName);

        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Activity {ActivityName} failed for request {RequestName}", requestName, requestName);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
        finally
        {
            _logger.LogTrace("Ending activity {ActivityName} for request {RequestName}", requestName, requestName);
        }
    }
}
