using Microsoft.Extensions.Logging;
using Quasar.Cqrs;

namespace Quasar.Security;

public interface IAuthorizableRequest
{
    Guid SubjectId { get; }
    string Action { get; }
    string Resource { get; }
}

public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger;
    public AuthorizationBehavior(IAuthorizationService authorizationService, ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

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
