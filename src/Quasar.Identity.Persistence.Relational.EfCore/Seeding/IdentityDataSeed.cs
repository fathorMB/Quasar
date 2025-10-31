using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quasar.EventSourcing.Abstractions;
using Quasar.Identity;
using Quasar.Seeding;
using Quasar.Identity.Persistence.Relational.EfCore;
using Quasar.Persistence.Relational.EfCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Quasar.Identity.Persistence.Relational.EfCore.Seeding;

public sealed class IdentityDataSeed : IOrderedDataSeed
{
    private readonly IdentitySeedOptions _options;
    private readonly ILogger<IdentityDataSeed> _logger;

    public IdentityDataSeed(IOptions<IdentitySeedOptions> options, ILogger<IdentityDataSeed> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public int Order => 200;

    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        if (_options.Sets.Count == 0)
        {
            _logger.LogDebug("No identity seed sets configured. Skipping identity seeding.");
            return;
        }

        var db = services.GetService<ReadModelContext<IdentityReadModelStore>>();
        if (db is null)
        {
            _logger.LogDebug("Identity read model context not available. Skipping identity seeding.");
            return;
        }

        var roleRepo = services.GetRequiredService<IEventSourcedRepository<RoleAggregate>>();
        var userRepo = services.GetRequiredService<IEventSourcedRepository<UserAggregate>>();
        var hasher = services.GetRequiredService<IPasswordHasher>();

        var roleLookup = (await db.Set<IdentityRoleReadModel>().AsNoTracking().ToListAsync(cancellationToken))
            .ToDictionary(r => r.Name, r => r.Id, StringComparer.OrdinalIgnoreCase);

        foreach (var set in _options.Sets)
        {
            var label = string.IsNullOrWhiteSpace(set.Name) ? "default" : set.Name!;
            _logger.LogInformation("Seeding identity set '{SeedSet}'", label);

            foreach (var role in set.Roles)
            {
                var roleId = ResolveRoleId(role, roleLookup);
                await EnsureRoleAsync(roleRepo, db, role, roleId, cancellationToken).ConfigureAwait(false);
                roleLookup[role.Name] = roleId;
            }

            foreach (var user in set.Users)
            {
                var resolvedRoleIds = await ResolveRoleIdsAsync(user, roleLookup, db, cancellationToken).ConfigureAwait(false);
                await EnsureUserAsync(userRepo, hasher, db, user, resolvedRoleIds, cancellationToken).ConfigureAwait(false);
            }
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static Guid ResolveRoleId(IdentityRoleSeed seed, IDictionary<string, Guid> roleLookup)
    {
        if (seed.Id != Guid.Empty) return seed.Id;
        if (!string.IsNullOrWhiteSpace(seed.Name) && roleLookup.TryGetValue(seed.Name, out var existing))
            return existing;
        return Guid.NewGuid();
    }

    private async Task EnsureRoleAsync(
        IEventSourcedRepository<RoleAggregate> repo,
        ReadModelContext<IdentityReadModelStore> db,
        IdentityRoleSeed seed,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var roles = db.Set<IdentityRoleReadModel>();
        var rolePermissions = db.Set<IdentityRolePermissionReadModel>();

        var aggregate = await repo.GetAsync(roleId, cancellationToken).ConfigureAwait(false);
        var isNew = aggregate.Id == Guid.Empty;

        if (isNew)
        {
            aggregate.Create(roleId, seed.Name);
            _logger.LogDebug("Created role {RoleName} ({RoleId})", seed.Name, roleId);
        }
        else if (!string.Equals(aggregate.Name, seed.Name, StringComparison.Ordinal))
        {
            aggregate.Rename(seed.Name);
            _logger.LogDebug("Renamed role {RoleId} to {RoleName}", roleId, seed.Name);
        }

        foreach (var permission in seed.Permissions.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            aggregate.GrantPermission(permission);
        }

        await repo.SaveAsync(aggregate, cancellationToken).ConfigureAwait(false);

        var roleModel = await roles.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken).ConfigureAwait(false);
        if (roleModel is null)
        {
            roleModel = new IdentityRoleReadModel { Id = roleId };
            await roles.AddAsync(roleModel, cancellationToken).ConfigureAwait(false);
        }
        roleModel.Name = seed.Name;

        foreach (var permission in seed.Permissions.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!rolePermissions.Local.Any(rp => rp.RoleId == roleId && string.Equals(rp.Permission, permission, StringComparison.OrdinalIgnoreCase)) &&
                !await rolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.Permission == permission, cancellationToken).ConfigureAwait(false))
            {
                await rolePermissions.AddAsync(new IdentityRolePermissionReadModel { RoleId = roleId, Permission = permission }, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static async Task<IReadOnlyCollection<Guid>> ResolveRoleIdsAsync(
        IdentityUserSeed seed,
        IDictionary<string, Guid> roleLookup,
        ReadModelContext<IdentityReadModelStore> db,
        CancellationToken cancellationToken)
    {
        var roles = db.Set<IdentityRoleReadModel>();
        var ids = new HashSet<Guid>(seed.Roles.Where(r => r != Guid.Empty));

        foreach (var roleName in seed.RoleNames)
        {
            if (roleLookup.TryGetValue(roleName, out var fromLookup))
            {
                ids.Add(fromLookup);
                continue;
            }

            var existing = await roles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken)
                .ConfigureAwait(false);

            if (existing is not null)
            {
                ids.Add(existing.Id);
                roleLookup[roleName] = existing.Id;
            }
        }

        return ids;
    }

    private async Task EnsureUserAsync(
        IEventSourcedRepository<UserAggregate> repo,
        IPasswordHasher hasher,
        ReadModelContext<IdentityReadModelStore> db,
        IdentityUserSeed seed,
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        var users = db.Set<IdentityUserReadModel>();
        var userRoles = db.Set<IdentityUserRoleReadModel>();

        var userId = seed.Id != Guid.Empty ? seed.Id : Guid.NewGuid();
        var aggregate = await repo.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        var isNew = aggregate.Id == Guid.Empty;

        if (isNew)
        {
            aggregate.Register(userId, seed.Username, seed.Email);
            _logger.LogDebug("Registered user {Username} ({UserId})", seed.Username, userId);
        }

        var userModel = await users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken).ConfigureAwait(false);
        if (userModel is null)
        {
            userModel = new IdentityUserReadModel { Id = userId };
            await users.AddAsync(userModel, cancellationToken).ConfigureAwait(false);
        }

        userModel.Username = seed.Username;
        userModel.Email = seed.Email;

        var passwordInfo = ResolvePassword(seed, hasher, isNew, userModel);
        if (passwordInfo.shouldUpdate && passwordInfo.hash is string hash && passwordInfo.salt is string salt)
        {
            aggregate.SetPassword(hash, salt);
            userModel.PasswordHash = hash;
            userModel.PasswordSalt = salt;
        }

        foreach (var roleId in roleIds)
        {
            if (roleId == Guid.Empty) continue;
            aggregate.AssignRole(roleId);

            if (!userRoles.Local.Any(ur => ur.UserId == userId && ur.RoleId == roleId) &&
                !await userRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken).ConfigureAwait(false))
            {
                await userRoles.AddAsync(new IdentityUserRoleReadModel { UserId = userId, RoleId = roleId }, cancellationToken).ConfigureAwait(false);
            }
        }

        await repo.SaveAsync(aggregate, cancellationToken).ConfigureAwait(false);
    }

    private static (string? hash, string? salt, bool shouldUpdate) ResolvePassword(
        IdentityUserSeed seed,
        IPasswordHasher hasher,
        bool isNewUser,
        IdentityUserReadModel userModel)
    {
        if (!string.IsNullOrWhiteSpace(seed.Password))
        {
            if (isNewUser || string.IsNullOrEmpty(userModel.PasswordHash) || string.IsNullOrEmpty(userModel.PasswordSalt))
            {
                var (hash, salt) = hasher.Hash(seed.Password);
                return (hash, salt, true);
            }

            return (null, null, false);
        }

        if (!string.IsNullOrWhiteSpace(seed.PasswordHash) && !string.IsNullOrWhiteSpace(seed.PasswordSalt))
        {
            var needsUpdate = string.IsNullOrEmpty(userModel.PasswordHash) ||
                              !string.Equals(userModel.PasswordHash, seed.PasswordHash, StringComparison.Ordinal) ||
                              string.IsNullOrEmpty(userModel.PasswordSalt) ||
                              !string.Equals(userModel.PasswordSalt, seed.PasswordSalt, StringComparison.Ordinal);
            return (seed.PasswordHash, seed.PasswordSalt, needsUpdate);
        }

        return (null, null, false);
    }
}
