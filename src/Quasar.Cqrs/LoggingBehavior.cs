using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Quasar.Cqrs;

/// <summary>
/// Pipeline behavior that logs start/end/failure for commands and queries.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
    {
        var requestName = request?.GetType().Name ?? typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("Handling {RequestName}", requestName);
            var response = await next().ConfigureAwait(false);
            sw.Stop();
            _logger.LogInformation("Handled {RequestName} in {Elapsed}ms", requestName, sw.Elapsed.TotalMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Error handling {RequestName} after {Elapsed}ms", requestName, sw.Elapsed.TotalMilliseconds);
            throw;
        }
    }
}
