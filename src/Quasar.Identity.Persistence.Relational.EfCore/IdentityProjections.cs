using Microsoft.EntityFrameworkCore;
using Quasar.EventSourcing.Abstractions;
using Quasar.Projections.Abstractions;

namespace Quasar.Identity.Persistence.Relational.EfCore;

public sealed class IdentityProjections :
    IProjection<UserRegistered>,
    IProjection<UserPasswordSet>,
    IProjection<UserRoleAssigned>,
    IProjection<UserRoleRevoked>,
    IProjection<RoleCreated>,
    IProjection<RolePermissionGranted>,
    IProjection<RolePermissionRevoked>
{
    private readonly IdentityReadModelContext _db;
    public IdentityProjections(IdentityReadModelContext db) => _db = db;

    public async Task HandleAsync(UserRegistered @event, CancellationToken cancellationToken = default)
    {
        var exists = await _db.Users.AnyAsync(x => x.Id == @event.UserId, cancellationToken);
        if (!exists)
        {
            _db.Users.Add(new IdentityUserReadModel
            {
                Id = @event.UserId,
                Username = @event.Username,
                Email = @event.Email
            });
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HandleAsync(UserPasswordSet @event, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == @event.UserId, cancellationToken);
        if (user is null)
        {
            user = new IdentityUserReadModel { Id = @event.UserId };
            _db.Users.Add(user);
        }
        user.PasswordHash = @event.PasswordHash;
        user.PasswordSalt = @event.PasswordSalt;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(UserRoleAssigned @event, CancellationToken cancellationToken = default)
    {
        var exists = await _db.UserRoles.AnyAsync(x => x.UserId == @event.UserId && x.RoleId == @event.RoleId, cancellationToken);
        if (!exists)
        {
            _db.UserRoles.Add(new IdentityUserRoleReadModel { UserId = @event.UserId, RoleId = @event.RoleId });
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HandleAsync(UserRoleRevoked @event, CancellationToken cancellationToken = default)
    {
        var entity = await _db.UserRoles.FirstOrDefaultAsync(x => x.UserId == @event.UserId && x.RoleId == @event.RoleId, cancellationToken);
        if (entity is not null)
        {
            _db.UserRoles.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HandleAsync(RoleCreated @event, CancellationToken cancellationToken = default)
    {
        var exists = await _db.Roles.AnyAsync(x => x.Id == @event.RoleId, cancellationToken);
        if (!exists)
        {
            _db.Roles.Add(new IdentityRoleReadModel { Id = @event.RoleId, Name = @event.Name });
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HandleAsync(RolePermissionGranted @event, CancellationToken cancellationToken = default)
    {
        var exists = await _db.RolePermissions.AnyAsync(x => x.RoleId == @event.RoleId && x.Permission == @event.Permission, cancellationToken);
        if (!exists)
        {
            _db.RolePermissions.Add(new IdentityRolePermissionReadModel { RoleId = @event.RoleId, Permission = @event.Permission });
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HandleAsync(RolePermissionRevoked @event, CancellationToken cancellationToken = default)
    {
        var entity = await _db.RolePermissions.FirstOrDefaultAsync(x => x.RoleId == @event.RoleId && x.Permission == @event.Permission, cancellationToken);
        if (entity is not null)
        {
            _db.RolePermissions.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
