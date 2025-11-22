using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Quasar.Identity.Persistence.Relational.EfCore;
using Quasar.Persistence.Relational.EfCore;
using Quasar.Security;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Quasar.Identity.Web;

public sealed class EfCoreAuthorizationService : IAuthorizationService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EfCoreAuthorizationService(IServiceScopeFactory scopeFactory, IHttpContextAccessor httpContextAccessor)
    {
        _scopeFactory = scopeFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> AuthorizeAsync(Guid subjectId, string action, string resource, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReadModelContext<IdentityReadModelStore>>();
        
        // Check session validity if sid claim is present
        var user = _httpContextAccessor.HttpContext?.User;
        var sidClaim = user?.FindFirst("sid")?.Value;
        
        Console.WriteLine($"[AuthDebug] Authorizing subject {subjectId} for {action} on {resource}. SID Claim: '{sidClaim}'");

        if (!string.IsNullOrEmpty(sidClaim) && Guid.TryParse(sidClaim, out var sessionId))
        {
            var session = await db.Set<IdentitySessionReadModel>()
                .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
            
            if (session is null) Console.WriteLine($"[AuthDebug] Session {sessionId} not found.");
            if (session?.RevokedUtc != null) Console.WriteLine($"[AuthDebug] Session {sessionId} is revoked at {session.RevokedUtc}.");
            if (session?.ExpiresUtc < DateTime.UtcNow) Console.WriteLine($"[AuthDebug] Session {sessionId} expired at {session.ExpiresUtc}.");

            if (session is null || session.RevokedUtc != null || session.ExpiresUtc < DateTime.UtcNow)
            {
                Console.WriteLine("[AuthDebug] Session invalid. Denying access.");
                return false; // Session is invalid or revoked
            }
        }
        else
        {
             Console.WriteLine("[AuthDebug] No SID claim found or invalid GUID.");
        }

        var userRoles = db.Set<IdentityUserRoleReadModel>();
        var rolePermissions = db.Set<IdentityRolePermissionReadModel>();

        var roleIds = await userRoles
            .Where(x => x.UserId == subjectId)
            .Select(x => x.RoleId)
            .ToArrayAsync(cancellationToken);

        if (roleIds.Length == 0)
            return false;

        return await rolePermissions.AnyAsync(x => roleIds.Contains(x.RoleId) && x.Permission == action, cancellationToken);
    }
}
