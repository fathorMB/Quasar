namespace Quasar.Security;

/// <summary>
/// Specifies whether an ACL entry grants or denies access.
/// </summary>
public enum AclEffect { Allow, Deny }

/// <summary>
/// Represents a rule that binds a subject, resource, and action to an access decision.
/// </summary>
/// <param name="SubjectId">Identifier of the principal the rule applies to.</param>
/// <param name="Resource">Resource name the rule covers.</param>
/// <param name="Action">The action performed on the resource.</param>
/// <param name="Effect">Determines whether the action is allowed or denied.</param>
public sealed record AclEntry(
    Guid SubjectId,
    string Resource,
    string Action,
    AclEffect Effect);

/// <summary>
/// Evaluates authorization policies for the framework.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Determines whether the specified subject may perform <paramref name="action"/> on <paramref name="resource"/>.
    /// </summary>
    Task<bool> AuthorizeAsync(Guid subjectId, string action, string resource, CancellationToken cancellationToken = default);
}

/// <summary>
/// Creates application tokens for authenticated subjects.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Creates a token for the provided <paramref name="subjectId"/> using optional <paramref name="claims"/> and <paramref name="lifetime"/>.
    /// </summary>
    string CreateToken(Guid subjectId, IReadOnlyDictionary<string, object>? claims = null, TimeSpan? lifetime = null);
}
