namespace Quasar.Security;

public enum AclEffect { Allow, Deny }

public sealed record AclEntry(
    Guid SubjectId,
    string Resource,
    string Action,
    AclEffect Effect);

public interface IAuthorizationService
{
    Task<bool> AuthorizeAsync(Guid subjectId, string action, string resource, CancellationToken cancellationToken = default);
}

public interface ITokenService
{
    string CreateToken(Guid subjectId, IReadOnlyDictionary<string, object>? claims = null, TimeSpan? lifetime = null);
}

