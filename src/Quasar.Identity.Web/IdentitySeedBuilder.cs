using Quasar.Identity.Persistence.Relational.EfCore.Seeding;

namespace Quasar.Identity.Web;

public sealed class IdentitySeedBuilder
{
    private readonly IdentitySeedSet _set;

    internal IdentitySeedBuilder(IdentitySeedSet set)
    {
        _set = set;
    }

    internal IdentitySeedSet GetSet() => _set;

    public IdentitySeedBuilder WithRole(string name, Action<IdentityRoleSeed>? configure = null)
    {
        var role = new IdentityRoleSeed { Name = name };
        configure?.Invoke(role);
        _set.Roles.Add(role);
        return this;
    }

    public IdentitySeedBuilder WithUser(string username, string email, string password, params string[] roleNames)
    {
        var user = new IdentityUserSeed
        {
            Username = username,
            Email = email,
            Password = password
        };
        foreach (var role in roleNames)
        {
            user.RoleNames.Add(role);
        }
        _set.Users.Add(user);
        return this;
    }
}
