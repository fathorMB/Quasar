namespace Quasar.Identity.Persistence.Relational.EfCore;

public sealed class IdentityUserReadModel
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
}

public sealed class IdentitySessionReadModel
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public DateTime IssuedUtc { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public DateTime? RevokedUtc { get; set; }
}

public sealed class IdentityRoleReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class IdentityRolePermissionReadModel
{
    public Guid RoleId { get; set; }
    public string Permission { get; set; } = string.Empty;
}

public sealed class IdentityUserRoleReadModel
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
