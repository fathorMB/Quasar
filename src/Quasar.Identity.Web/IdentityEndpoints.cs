using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Quasar.Cqrs;
using Quasar.Identity.Persistence.Relational.EfCore;
using Quasar.Persistence.Relational.EfCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace Quasar.Identity.Web;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "quasar";
    public string Audience { get; set; } = "quasar";
    public string Key { get; set; } = "dev-secret-change";
    public int AccessMinutes { get; set; } = 15;
    public int RefreshDays { get; set; } = 7;
}

public interface IJwtTokenService
{
    (string accessToken, DateTime expiresUtc) CreateAccessToken(Guid userId, string username, IEnumerable<string> roles);
    (string refreshToken, DateTime expiresUtc, string hash) CreateRefreshToken(JwtOptions options);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _opt;
    private readonly SigningCredentials _cred;
    public JwtTokenService(IOptions<JwtOptions> opt)
    {
        _opt = opt.Value;
        _cred = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key)), SecurityAlgorithms.HmacSha256);
    }

    public (string accessToken, DateTime expiresUtc) CreateAccessToken(Guid userId, string username, IEnumerable<string> roles)
    {
        var handler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_opt.AccessMinutes);
        var token = new JwtSecurityToken(
            _opt.Issuer,
            _opt.Audience,
            new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, username)
            }.Concat(roles.Select(r => new Claim(ClaimTypes.Role, r))),
            notBefore: now,
            expires: expires,
            signingCredentials: _cred);
        return (handler.WriteToken(token), expires);
    }

    public (string refreshToken, DateTime expiresUtc, string hash) CreateRefreshToken(JwtOptions options)
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        var token = Convert.ToBase64String(bytes);
        var hashBytes = SHA256.HashData(bytes);
        var hash = Convert.ToBase64String(hashBytes);
        var expires = DateTime.UtcNow.AddDays(options.RefreshDays);
        return (token, expires, hash);
    }
}

public static class IdentityEndpoints
{
    private static IdentityDbSets GetSets(ReadModelContext<IdentityReadModelStore> db) => new(
        db.Set<IdentityUserReadModel>(),
        db.Set<IdentityRoleReadModel>(),
        db.Set<IdentityRolePermissionReadModel>(),
        db.Set<IdentityUserRoleReadModel>(),
        db.Set<IdentitySessionReadModel>());

    private sealed record IdentityDbSets(
        DbSet<IdentityUserReadModel> Users,
        DbSet<IdentityRoleReadModel> Roles,
        DbSet<IdentityRolePermissionReadModel> RolePermissions,
        DbSet<IdentityUserRoleReadModel> UserRoles,
        DbSet<IdentitySessionReadModel> Sessions);

    private static bool TryComputeRefreshHash(string token, out string hash)
    {
        hash = string.Empty;
        try
        {
            var bytes = Convert.FromBase64String(token);
            hash = Convert.ToBase64String(SHA256.HashData(bytes));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static IServiceCollection AddQuasarIdentity(this IServiceCollection services, Action<JwtOptions>? configure = null)
    {
        services.AddReadModelDefinition<IdentityReadModelDefinition>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<ICommandHandler<RegisterUserCommand, Guid>, RegisterUserHandler>();
        services.AddScoped<ICommandHandler<CreateRoleCommand, Guid>, CreateRoleHandler>();
        services.AddScoped<ICommandHandler<RenameRoleCommand, bool>, RenameRoleHandler>();
        services.AddScoped<ICommandHandler<GrantRolePermissionCommand, bool>, GrantRolePermissionHandler>();
        services.AddScoped<ICommandHandler<RevokeRolePermissionCommand, bool>, RevokeRolePermissionHandler>();
        services.AddScoped<ICommandHandler<AssignRoleToUserCommand, bool>, AssignRoleToUserHandler>();
        services.AddScoped<ICommandHandler<RevokeRoleFromUserCommand, bool>, RevokeRoleFromUserHandler>();
        services.AddScoped<Quasar.Security.IAuthorizationService, EfCoreAuthorizationService>();
        services.AddOptions<JwtOptions>();
        if (configure != null) services.Configure(configure);
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        return services;
    }

    public static IEndpointRouteBuilder MapQuasarIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (IMediator mediator, IPasswordHasher hasher, ReadModelContext<IdentityReadModelStore> db, RegisterDto dto) =>
        {
            var sets = GetSets(db);
            var users = sets.Users;

            // Basic uniqueness check (read model side)
            if (await users.AnyAsync(u => u.Username == dto.Username).ConfigureAwait(false))
                return Results.Conflict("Username already exists");

            var id = await mediator.Send(new RegisterUserCommand(dto.Username, dto.Email, dto.Password)).ConfigureAwait(false);

            // Ensure read model is immediately usable for login (projection may be async)
            var user = await users.FirstOrDefaultAsync(u => u.Id == id).ConfigureAwait(false);
            if (user is null)
            {
                user = new IdentityUserReadModel { Id = id, Username = dto.Username, Email = dto.Email };
                await users.AddAsync(user).ConfigureAwait(false);
            }

            var (hash, salt) = hasher.Hash(dto.Password);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            await db.SaveChangesAsync().ConfigureAwait(false);
            return Results.Ok(new { userId = id });
        });

        app.MapPost("/auth/login", async (IJwtTokenService tokens, IPasswordHasher hasher, ReadModelContext<IdentityReadModelStore> db, IOptions<JwtOptions> optAccessor, LoginDto dto) =>
        {
            var sets = GetSets(db);
            var users = sets.Users;
            var userRoles = sets.UserRoles;
            var roles = sets.Roles;
            var sessions = sets.Sessions;

            var user = await users.FirstOrDefaultAsync(u => u.Username == dto.Username).ConfigureAwait(false);
            if (user is null) return Results.Unauthorized();
            if (!hasher.Verify(dto.Password, user.PasswordHash, user.PasswordSalt)) return Results.Unauthorized();
            var roleNames = await userRoles
                .Where(x => x.UserId == user.Id)
                .Join(roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToArrayAsync()
                .ConfigureAwait(false);
            var (access, expires) = tokens.CreateAccessToken(user.Id, user.Username, roleNames);
            var (refresh, refreshExpires, hash) = tokens.CreateRefreshToken(optAccessor.Value);
            var session = await sessions.FirstOrDefaultAsync(s => s.UserId == user.Id).ConfigureAwait(false);
            if (session is null)
            {
                session = new IdentitySessionReadModel { SessionId = Guid.NewGuid(), UserId = user.Id };
                await sessions.AddAsync(session).ConfigureAwait(false);
            }
            session.RefreshTokenHash = hash;
            session.IssuedUtc = DateTime.UtcNow;
            session.ExpiresUtc = refreshExpires;
            session.RevokedUtc = null;
            await db.SaveChangesAsync().ConfigureAwait(false);
            return Results.Ok(new { accessToken = access, accessExpiresUtc = expires, refreshToken = refresh, refreshExpiresUtc = refreshExpires });
        });

        app.MapPost("/auth/roles", async (IMediator mediator, ReadModelContext<IdentityReadModelStore> db, CreateRoleRequest dto) =>
        {
            var sets = GetSets(db);
            var roles = sets.Roles;
            var roleId = await mediator.Send(new CreateRoleCommand(dto.Name)).ConfigureAwait(false);
            if (!await roles.AnyAsync(r => r.Id == roleId).ConfigureAwait(false))
            {
                await roles.AddAsync(new IdentityRoleReadModel { Id = roleId, Name = dto.Name }).ConfigureAwait(false);
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
            return Results.Ok(new { roleId });
        });

        app.MapPost("/auth/roles/{roleId:guid}/permissions", async (IMediator mediator, ReadModelContext<IdentityReadModelStore> db, Guid roleId, GrantPermissionRequest dto) =>
        {
            var sets = GetSets(db);
            var rolePermissions = sets.RolePermissions;
            var result = await mediator.Send(new GrantRolePermissionCommand(roleId, dto.Permission)).ConfigureAwait(false);
            if (result)
            {
                if (!await rolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.Permission == dto.Permission).ConfigureAwait(false))
                {
                    await rolePermissions.AddAsync(new IdentityRolePermissionReadModel { RoleId = roleId, Permission = dto.Permission }).ConfigureAwait(false);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
            }
            return result ? Results.Ok() : Results.NotFound();
        });

        app.MapDelete("/auth/roles/{roleId:guid}/permissions/{permission}", async (IMediator mediator, ReadModelContext<IdentityReadModelStore> db, Guid roleId, string permission) =>
        {
            var sets = GetSets(db);
            var rolePermissions = sets.RolePermissions;
            var result = await mediator.Send(new RevokeRolePermissionCommand(roleId, permission)).ConfigureAwait(false);
            if (result)
            {
                var entity = await rolePermissions.FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.Permission == permission).ConfigureAwait(false);
                if (entity is not null)
                {
                    rolePermissions.Remove(entity);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
            }
            return result ? Results.Ok() : Results.NotFound();
        });

        app.MapPost("/auth/users/{userId:guid}/roles", async (IMediator mediator, ReadModelContext<IdentityReadModelStore> db, Guid userId, AssignUserRoleRequest dto) =>
        {
            var sets = GetSets(db);
            var userRoles = sets.UserRoles;
            var result = await mediator.Send(new AssignRoleToUserCommand(userId, dto.RoleId)).ConfigureAwait(false);
            if (result)
            {
                if (!await userRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == dto.RoleId).ConfigureAwait(false))
                {
                    await userRoles.AddAsync(new IdentityUserRoleReadModel { UserId = userId, RoleId = dto.RoleId }).ConfigureAwait(false);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
            }
            return result ? Results.Ok() : Results.NotFound();
        });

        app.MapDelete("/auth/users/{userId:guid}/roles/{roleId:guid}", async (IMediator mediator, ReadModelContext<IdentityReadModelStore> db, Guid userId, Guid roleId) =>
        {
            var sets = GetSets(db);
            var userRoles = sets.UserRoles;
            var result = await mediator.Send(new RevokeRoleFromUserCommand(userId, roleId)).ConfigureAwait(false);
            if (result)
            {
                var entity = await userRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId).ConfigureAwait(false);
                if (entity is not null)
                {
                    userRoles.Remove(entity);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
            }
            return result ? Results.Ok() : Results.NotFound();
        });

        app.MapGet("/auth/users/{userId:guid}/roles", async (ReadModelContext<IdentityReadModelStore> db, Guid userId) =>
        {
            var sets = GetSets(db);
            var roles = await sets.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(sets.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { r.Id, r.Name })
                .ToListAsync()
                .ConfigureAwait(false);
            return Results.Ok(roles);
        });

        app.MapGet("/auth/users/{userId:guid}/permissions", async (ReadModelContext<IdentityReadModelStore> db, Guid userId) =>
        {
            var sets = GetSets(db);
            var permissions = await sets.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(sets.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.Permission)
                .Distinct()
                .ToListAsync()
                .ConfigureAwait(false);
            return Results.Ok(permissions);
        });

        app.MapGet("/auth/roles/{roleId:guid}/permissions", async (ReadModelContext<IdentityReadModelStore> db, Guid roleId) =>
        {
            var sets = GetSets(db);
            var perms = await sets.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permission)
                .ToListAsync()
                .ConfigureAwait(false);
            return Results.Ok(perms);
        });

        app.MapGet("/auth/roles", async (ReadModelContext<IdentityReadModelStore> db) =>
        {
            var sets = GetSets(db);
            var roles = await sets.Roles.ToListAsync().ConfigureAwait(false);
            return Results.Ok(roles);
        });

        app.MapGet("/auth/users", async (ReadModelContext<IdentityReadModelStore> db) =>
        {
            var sets = GetSets(db);
            var users = await sets.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email
                })
                .ToListAsync()
                .ConfigureAwait(false);
            return Results.Ok(users);
        });

        app.MapGet("/auth/acl", async (ReadModelContext<IdentityReadModelStore> db) =>
        {
            var sets = GetSets(db);
            var assignments = await sets.UserRoles
                .Join(sets.Users, ur => ur.UserId, u => u.Id, (ur, u) => new { ur, u })
                .Join(sets.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new
                {
                    UserId = x.u.Id,
                    x.u.Username,
                    RoleId = r.Id,
                    RoleName = r.Name
                })
                .ToListAsync()
                .ConfigureAwait(false);
            return Results.Ok(assignments);
        });

        app.MapPost("/auth/token/refresh", async (IJwtTokenService tokens, ReadModelContext<IdentityReadModelStore> db, IOptions<JwtOptions> optAccessor, RefreshRequest dto) =>
        {
            if (!TryComputeRefreshHash(dto.RefreshToken, out var hash)) return Results.Unauthorized();
            var sets = GetSets(db);
            var session = await sets.Sessions.FirstOrDefaultAsync(s => s.RefreshTokenHash == hash && s.RevokedUtc == null).ConfigureAwait(false);
            if (session is null) return Results.Unauthorized();
            if (session.ExpiresUtc < DateTime.UtcNow) return Results.Unauthorized();

            var user = await sets.Users.FirstOrDefaultAsync(u => u.Id == session.UserId).ConfigureAwait(false);
            if (user is null) return Results.Unauthorized();

            var roleNames = await sets.UserRoles
                .Where(x => x.UserId == user.Id)
                .Join(sets.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToArrayAsync()
                .ConfigureAwait(false);

            var (access, accessExpires) = tokens.CreateAccessToken(user.Id, user.Username, roleNames);
            var (refresh, refreshExpires, newHash) = tokens.CreateRefreshToken(optAccessor.Value);

            session.RefreshTokenHash = newHash;
            session.IssuedUtc = DateTime.UtcNow;
            session.ExpiresUtc = refreshExpires;
            await db.SaveChangesAsync().ConfigureAwait(false);

            return Results.Ok(new { accessToken = access, accessExpiresUtc = accessExpires, refreshToken = refresh, refreshExpiresUtc = refreshExpires });
        });

        app.MapPost("/auth/logout", async (ReadModelContext<IdentityReadModelStore> db, LogoutRequest dto) =>
        {
            if (!TryComputeRefreshHash(dto.RefreshToken, out var hash)) return Results.Ok();
            var sets = GetSets(db);
            var session = await sets.Sessions.FirstOrDefaultAsync(s => s.RefreshTokenHash == hash && s.RevokedUtc == null).ConfigureAwait(false);
            if (session is null) return Results.Ok();
            session.RevokedUtc = DateTime.UtcNow;
            await db.SaveChangesAsync().ConfigureAwait(false);
            return Results.Ok();
        });

        return app;
    }
}

public sealed record RegisterDto(string Username, string Email, string Password);
public sealed record LoginDto(string Username, string Password);
public sealed record CreateRoleRequest(string Name);
public sealed record GrantPermissionRequest(string Permission);
public sealed record AssignUserRoleRequest(Guid RoleId);
public sealed record RefreshRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);
















