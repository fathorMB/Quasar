using Microsoft.EntityFrameworkCore;
using Quasar.Persistence.Relational.EfCore;

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

public sealed class IdentityReadModelContext : ReadModelContext
{
    public IdentityReadModelContext(DbContextOptions<IdentityReadModelContext> options) : base(options) { }
    public DbSet<IdentityUserReadModel> Users => Set<IdentityUserReadModel>();
    public DbSet<IdentitySessionReadModel> Sessions => Set<IdentitySessionReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityUserReadModel>(e =>
        {
            e.HasKey(x => x.Id);
            e.ToTable("Users");
            e.Property(x => x.Username).IsRequired();
            e.Property(x => x.Email).IsRequired();
        });
        modelBuilder.Entity<IdentitySessionReadModel>(e =>
        {
            e.HasKey(x => x.SessionId);
            e.ToTable("Sessions");
        });
    }
}
