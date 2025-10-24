using Microsoft.Extensions.Logging;
using Quasar.Cqrs;

namespace Quasar.Security;

/// <summary>
/// Represents a request that requires authorization before execution.
/// </summary>
public interface IAuthorizableRequest
{
    /// <summary>
    /// Gets the subject performing the request.
    /// </summary>
    Guid SubjectId { get; }

    /// <summary>
    /// Gets the action the subject wants to perform.
    /// </summary>
    string Action { get; }

    /// <summary>
    /// Gets the resource the action targets.
    /// </summary>
    string Resource { get; }
}

/// <summary>
/// Mediator behavior that enforces authorization via <see cref="IAuthorizationService"/> for requests implementing <see cref="IAuthorizableRequest"/>.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    public AuthorizationBehavior(IAuthorizationService authorizationService, ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
    {
        if (request is IAuthorizableRequest ar)
        {
            _logger.LogTrace("Authorizing action {Action} on {Resource} for subject {Subject}", ar.Action, ar.Resource, ar.SubjectId);
            var allowed = await _authorizationService.AuthorizeAsync(ar.SubjectId, ar.Action, ar.Resource, cancellationToken).ConfigureAwait(false);
            if (!allowed)
            {
                _logger.LogWarning("Authorization failed for subject {Subject} on {Action} {Resource}", ar.SubjectId, ar.Action, ar.Resource);
                throw new UnauthorizedAccessException($"Subject '{ar.SubjectId}' not authorized for {ar.Action} on {ar.Resource}.");
            }
            _logger.LogDebug("Authorization succeeded for subject {Subject} on {Action} {Resource}", ar.SubjectId, ar.Action, ar.Resource);
        }
        return await next().ConfigureAwait(false);
    }
}
