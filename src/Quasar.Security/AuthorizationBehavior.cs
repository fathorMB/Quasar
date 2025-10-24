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
    public AuthorizationBehavior(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
    {
        if (request is IAuthorizableRequest ar)
        {
            var allowed = await _authorizationService.AuthorizeAsync(ar.SubjectId, ar.Action, ar.Resource, cancellationToken).ConfigureAwait(false);
            if (!allowed)
                throw new UnauthorizedAccessException($"Subject '{ar.SubjectId}' not authorized for {ar.Action} on {ar.Resource}.");
        }
        return await next().ConfigureAwait(false);
    }
}

