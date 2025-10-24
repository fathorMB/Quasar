using Microsoft.EntityFrameworkCore;
using Quasar.Identity.Persistence.Relational.EfCore;
using Quasar.Security;

namespace Quasar.Identity.Web;

public sealed class EfCoreAuthorizationService : IAuthorizationService
{
    private readonly IdentityReadModelContext _db;
    public EfCoreAuthorizationService(IdentityReadModelContext db) => _db = db;

    public async Task<bool> AuthorizeAsync(Guid subjectId, string action, string resource, CancellationToken cancellationToken = default)
    {
        var roleIds = await _db.UserRoles
            .Where(x => x.UserId == subjectId)
            .Select(x => x.RoleId)
            .ToArrayAsync(cancellationToken);

        if (roleIds.Length == 0)
            return false;

        return await _db.RolePermissions.AnyAsync(x => roleIds.Contains(x.RoleId) && x.Permission == action, cancellationToken);
    }
}
