using System;
using System.Collections.Generic;

namespace Quasar.Identity.Persistence.Relational.EfCore.Seeding;

public sealed class IdentitySeedOptions
{
    public IList<IdentitySeedSet> Sets { get; } = new List<IdentitySeedSet>();
}

public sealed class IdentitySeedSet
{
    public string? Name { get; set; }
    public IList<IdentityRoleSeed> Roles { get; init; } = new List<IdentityRoleSeed>();
    public IList<IdentityUserSeed> Users { get; init; } = new List<IdentityUserSeed>();
}

public sealed class IdentityRoleSeed
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IList<string> Permissions { get; init; } = new List<string>();
}

public sealed class IdentityUserSeed
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? PasswordHash { get; set; }
    public string? PasswordSalt { get; set; }
    public IList<Guid> Roles { get; init; } = new List<Guid>();
    public IList<string> RoleNames { get; init; } = new List<string>();
}
