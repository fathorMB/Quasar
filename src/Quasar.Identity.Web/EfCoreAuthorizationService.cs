using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quasar.Identity.Persistence.Relational.EfCore;
using Quasar.Persistence.Relational.EfCore;
using Quasar.Security;

namespace Quasar.Identity.Web;

public sealed class EfCoreAuthorizationService : IAuthorizationService
{
    private readonly IServiceScopeFactory _scopeFactory;
    public EfCoreAuthorizationService(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public async Task<bool> AuthorizeAsync(Guid subjectId, string action, string resource, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReadModelContext<IdentityReadModelStore>>();
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
