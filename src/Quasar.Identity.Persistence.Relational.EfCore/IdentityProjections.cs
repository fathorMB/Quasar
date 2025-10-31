using Microsoft.EntityFrameworkCore;
using Quasar.EventSourcing.Abstractions;
using Quasar.Persistence.Relational.EfCore;
using Quasar.Projections.Abstractions;

namespace Quasar.Identity.Persistence.Relational.EfCore;

public sealed class IdentityProjections :
    IProjection<UserRegistered>,
    IProjection<UserPasswordSet>,
    IProjection<UserRoleAssigned>,
    IProjection<UserRoleRevoked>,
    IProjection<RoleCreated>,
    IProjection<RoleRenamed>,
    IProjection<RolePermissionGranted>,
    IProjection<RolePermissionRevoked>
{
    private readonly ReadModelContext<IdentityReadModelStore> _db;
    private readonly DbSet<IdentityUserReadModel> _users;
    private readonly DbSet<IdentitySessionReadModel> _sessions;
    private readonly DbSet<IdentityRoleReadModel> _roles;
    private readonly DbSet<IdentityRolePermissionReadModel> _rolePermissions;
    private readonly DbSet<IdentityUserRoleReadModel> _userRoles;

    public IdentityProjections(ReadModelContext<IdentityReadModelStore> db)
    {
        _db = db;
        _users = db.Set<IdentityUserReadModel>();
        _sessions = db.Set<IdentitySessionReadModel>();
        _roles = db.Set<IdentityRoleReadModel>();
        _rolePermissions = db.Set<IdentityRolePermissionReadModel>();
        _userRoles = db.Set<IdentityUserRoleReadModel>();
    }

    public async Task HandleAsync(UserRegistered @event, CancellationToken cancellationToken = default)
    {
        var exists = await _users.AnyAsync(x => x.Id == @event.UserId, cancellationToken);
        if (!exists)
        {
            await _users.AddAsync(new IdentityUserReadModel
            {
                Id = @event.UserId,
                Username = @event.Username,
                Email = @event.Email
            }, cancellationToken).ConfigureAwait(false);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HandleAsync(UserPasswordSet @event, CancellationToken cancellationToken = default)
    {
        var user = await _users.FirstOrDefaultAsync(x => x.Id == @event.UserId, cancellationToken);
        if (user is null)
        {
            user = new IdentityUserReadModel { Id = @event.UserId };
            await _users.AddAsync(user, cancellationToken).ConfigureAwait(false);
        }
        user.PasswordHash = @event.PasswordHash;
        user.PasswordSalt = @event.PasswordSalt;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(UserRoleAssigned @event, CancellationToken cancellationToken = default)
    {
        var exists = await _userRoles.AnyAsync(x => x.UserId == @event.UserId && x.RoleId == @event.RoleId, cancellationToken);
        if (!exists)
        {
            await _userRoles.AddAsync(new IdentityUserRoleReadModel { UserId = @event.UserId, RoleId = @event.RoleId }, cancellationToken).ConfigureAwait(false);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HandleAsync(UserRoleRevoked @event, CancellationToken cancellationToken = default)
    {
        var entity = await _userRoles.FirstOrDefaultAsync(x => x.UserId == @event.UserId && x.RoleId == @event.RoleId, cancellationToken);
        if (entity is not null)
        {
            _userRoles.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HandleAsync(RoleCreated @event, CancellationToken cancellationToken = default)
    {
        var exists = await _roles.AnyAsync(x => x.Id == @event.RoleId, cancellationToken);
        if (!exists)
        {
            await _roles.AddAsync(new IdentityRoleReadModel { Id = @event.RoleId, Name = @event.Name }, cancellationToken).ConfigureAwait(false);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HandleAsync(RoleRenamed @event, CancellationToken cancellationToken = default)
    {
        var role = await _roles.FirstOrDefaultAsync(x => x.Id == @event.RoleId, cancellationToken);
        if (role is not null)
        {
            role.Name = @event.Name;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HandleAsync(RolePermissionGranted @event, CancellationToken cancellationToken = default)
    {
        var exists = await _rolePermissions.AnyAsync(x => x.RoleId == @event.RoleId && x.Permission == @event.Permission, cancellationToken);
        if (!exists)
        {
            await _rolePermissions.AddAsync(new IdentityRolePermissionReadModel { RoleId = @event.RoleId, Permission = @event.Permission }, cancellationToken).ConfigureAwait(false);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HandleAsync(RolePermissionRevoked @event, CancellationToken cancellationToken = default)
    {
        var entity = await _rolePermissions.FirstOrDefaultAsync(x => x.RoleId == @event.RoleId && x.Permission == @event.Permission, cancellationToken);
        if (entity is not null)
        {
            _rolePermissions.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
