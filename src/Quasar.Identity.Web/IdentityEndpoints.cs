using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Quasar.Cqrs;
using Quasar.Identity.Persistence.Relational.EfCore;
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
        app.MapPost("/auth/register", async (IMediator mediator, IPasswordHasher hasher, IdentityReadModelContext db, RegisterDto dto) =>
        {
            // Basic uniqueness check (read model side)
            if (await db.Users.AnyAsync(u => u.Username == dto.Username))
                return Results.Conflict("Username already exists");

            var id = await mediator.Send(new RegisterUserCommand(dto.Username, dto.Email, dto.Password));
            // Ensure read model is immediately usable for login (projection may be async)
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null)
            {
                user = new IdentityUserReadModel { Id = id, Username = dto.Username, Email = dto.Email };
                db.Users.Add(user);
            }
            var (hash, salt) = hasher.Hash(dto.Password);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            await db.SaveChangesAsync();
            return Results.Ok(new { userId = id });
        });

        app.MapPost("/auth/login", async (IJwtTokenService tokens, IPasswordHasher hasher, IdentityReadModelContext db, IOptions<JwtOptions> optAccessor, LoginDto dto) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user is null) return Results.Unauthorized();
            if (!hasher.Verify(dto.Password, user.PasswordHash, user.PasswordSalt)) return Results.Unauthorized();
            var roleNames = await db.UserRoles
                .Where(x => x.UserId == user.Id)
                .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToArrayAsync();
            var (access, expires) = tokens.CreateAccessToken(user.Id, user.Username, roleNames);
            var (refresh, refreshExpires, hash) = tokens.CreateRefreshToken(optAccessor.Value);
            db.Sessions.Add(new IdentitySessionReadModel
            {
                SessionId = Guid.NewGuid(),
                UserId = user.Id,
                RefreshTokenHash = hash,
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = refreshExpires
            });
            await db.SaveChangesAsync();
            return Results.Ok(new { accessToken = access, accessExpiresUtc = expires, refreshToken = refresh, refreshExpiresUtc = refreshExpires });
        });

        app.MapPost("/auth/roles", async (IMediator mediator, IdentityReadModelContext db, CreateRoleRequest dto) =>
        {
            var roleId = await mediator.Send(new CreateRoleCommand(dto.Name));
            if (!await db.Roles.AnyAsync(r => r.Id == roleId))
            {
                db.Roles.Add(new IdentityRoleReadModel { Id = roleId, Name = dto.Name });
                await db.SaveChangesAsync();
            }
            return Results.Ok(new { roleId });
        });

        app.MapPost("/auth/roles/{roleId:guid}/permissions", async (IMediator mediator, IdentityReadModelContext db, Guid roleId, GrantPermissionRequest dto) =>
        {
            var result = await mediator.Send(new GrantRolePermissionCommand(roleId, dto.Permission));
            if (result)
            {
                if (!await db.RolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.Permission == dto.Permission))
                {
                    db.RolePermissions.Add(new IdentityRolePermissionReadModel { RoleId = roleId, Permission = dto.Permission });
                    await db.SaveChangesAsync();
                }
            }
            return result ? Results.Ok() : Results.NotFound();
        });

        app.MapDelete("/auth/roles/{roleId:guid}/permissions/{permission}", async (IMediator mediator, IdentityReadModelContext db, Guid roleId, string permission) =>
        {
            var result = await mediator.Send(new RevokeRolePermissionCommand(roleId, permission));
            if (result)
            {
                var entity = await db.RolePermissions.FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.Permission == permission);
                if (entity is not null)
                {
                    db.RolePermissions.Remove(entity);
                    await db.SaveChangesAsync();
                }
            }
            return result ? Results.Ok() : Results.NotFound();
        });

        app.MapPost("/auth/users/{userId:guid}/roles", async (IMediator mediator, IdentityReadModelContext db, Guid userId, AssignUserRoleRequest dto) =>
        {
            var result = await mediator.Send(new AssignRoleToUserCommand(userId, dto.RoleId));
            if (result)
            {
                if (!await db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == dto.RoleId))
                {
                    db.UserRoles.Add(new IdentityUserRoleReadModel { UserId = userId, RoleId = dto.RoleId });
                    await db.SaveChangesAsync();
                }
            }
            return result ? Results.Ok() : Results.NotFound();
        });

        app.MapDelete("/auth/users/{userId:guid}/roles/{roleId:guid}", async (IMediator mediator, IdentityReadModelContext db, Guid userId, Guid roleId) =>
        {
            var result = await mediator.Send(new RevokeRoleFromUserCommand(userId, roleId));
            if (result)
            {
                var entity = await db.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
                if (entity is not null)
                {
                    db.UserRoles.Remove(entity);
                    await db.SaveChangesAsync();
                }
            }
            return result ? Results.Ok() : Results.NotFound();
        });

        app.MapGet("/auth/users/{userId:guid}/roles", async (IdentityReadModelContext db, Guid userId) =>
        {
            var roles = await db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { r.Id, r.Name })
                .ToListAsync();
            return Results.Ok(roles);
        });

        app.MapGet("/auth/users/{userId:guid}/permissions", async (IdentityReadModelContext db, Guid userId) =>
        {
            var permissions = await db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(db.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.Permission)
                .Distinct()
                .ToListAsync();
            return Results.Ok(permissions);
        });

        app.MapGet("/auth/roles/{roleId:guid}/permissions", async (IdentityReadModelContext db, Guid roleId) =>
        {
            var perms = await db.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permission)
                .ToListAsync();
            return Results.Ok(perms);
        });

        app.MapGet("/auth/roles", async (IdentityReadModelContext db) =>
        {
            var roles = await db.Roles.ToListAsync();
            return Results.Ok(roles);
        });

        app.MapGet("/auth/users", async (IdentityReadModelContext db) =>
        {
            var users = await db.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email
                })
                .ToListAsync();
            return Results.Ok(users);
        });

        app.MapGet("/auth/acl", async (IdentityReadModelContext db) =>
        {
            var assignments = await db.UserRoles
                .Join(db.Users, ur => ur.UserId, u => u.Id, (ur, u) => new { ur, u })
                .Join(db.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new
                {
                    UserId = x.u.Id,
                    x.u.Username,
                    RoleId = r.Id,
                    RoleName = r.Name
                })
                .ToListAsync();
            return Results.Ok(assignments);
        });

        app.MapPost("/auth/token/refresh", async (IJwtTokenService tokens, IdentityReadModelContext db, IOptions<JwtOptions> optAccessor, RefreshRequest dto) =>
        {
            if (!TryComputeRefreshHash(dto.RefreshToken, out var hash)) return Results.Unauthorized();
            var session = await db.Sessions.FirstOrDefaultAsync(s => s.RefreshTokenHash == hash && s.RevokedUtc == null);
            if (session is null) return Results.Unauthorized();
            if (session.ExpiresUtc < DateTime.UtcNow) return Results.Unauthorized();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == session.UserId);
            if (user is null) return Results.Unauthorized();

            var roleNames = await db.UserRoles
                .Where(x => x.UserId == user.Id)
                .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToArrayAsync();

            var (access, accessExpires) = tokens.CreateAccessToken(user.Id, user.Username, roleNames);
            var (refresh, refreshExpires, newHash) = tokens.CreateRefreshToken(optAccessor.Value);

            session.RefreshTokenHash = newHash;
            session.IssuedUtc = DateTime.UtcNow;
            session.ExpiresUtc = refreshExpires;
            await db.SaveChangesAsync();

            return Results.Ok(new { accessToken = access, accessExpiresUtc = accessExpires, refreshToken = refresh, refreshExpiresUtc = refreshExpires });
        });

        app.MapPost("/auth/logout", async (IdentityReadModelContext db, LogoutRequest dto) =>
        {
            if (!TryComputeRefreshHash(dto.RefreshToken, out var hash)) return Results.Ok();
            var session = await db.Sessions.FirstOrDefaultAsync(s => s.RefreshTokenHash == hash && s.RevokedUtc == null);
            if (session is null) return Results.Ok();
            session.RevokedUtc = DateTime.UtcNow;
            await db.SaveChangesAsync();
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
