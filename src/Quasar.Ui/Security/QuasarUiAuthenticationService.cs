using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quasar.Identity;
using Quasar.Identity.Persistence.Relational.EfCore;
using Quasar.Persistence.Relational.EfCore;

namespace Quasar.Ui.Security;

public sealed class QuasarUiAuthenticationService
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ReadModelContext<IdentityReadModelStore> _db;
    private readonly QuasarUiSecurityOptions _options;

    public QuasarUiAuthenticationService(
        IHttpContextAccessor contextAccessor,
        IPasswordHasher passwordHasher,
        ReadModelContext<IdentityReadModelStore> db,
        QuasarUiSecurityOptions options)
    {
        _contextAccessor = contextAccessor;
        _passwordHasher = passwordHasher;
        _db = db;
        _options = options;
    }

    public async Task<bool> SignInAsync(string username, string password, bool rememberMe, CancellationToken cancellationToken = default)
    {
        var httpContext = _contextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP context.");

        var users = _db.Set<IdentityUserReadModel>();
        var userRoles = _db.Set<IdentityUserRoleReadModel>();
        var roles = _db.Set<IdentityRoleReadModel>();

        var user = await users.FirstOrDefaultAsync(x => x.Username == username, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return false;
        }

        if (!_passwordHasher.Verify(password, user.PasswordHash ?? string.Empty, user.PasswordSalt ?? string.Empty))
        {
            return false;
        }

        var roleNames = await userRoles
            .Where(x => x.UserId == user.Id)
            .Join(roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username)
        };
        foreach (var role in roleNames)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, QuasarUiAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.Add(_options.SessionDuration)
        };
        await httpContext.SignInAsync(QuasarUiAuthenticationDefaults.AuthenticationScheme, principal, properties).ConfigureAwait(false);
        return true;
    }

    public Task SignOutAsync()
    {
        var httpContext = _contextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP context.");
        return httpContext.SignOutAsync(QuasarUiAuthenticationDefaults.AuthenticationScheme);
    }
}
