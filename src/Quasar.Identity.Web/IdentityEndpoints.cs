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
}

public static class IdentityEndpoints
{
    public static IServiceCollection AddQuasarIdentity(this IServiceCollection services, Action<JwtOptions>? configure = null)
    {
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<ICommandHandler<RegisterUserCommand, Guid>, RegisterUserHandler>();
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

        app.MapPost("/auth/login", async (IJwtTokenService tokens, IPasswordHasher hasher, IdentityReadModelContext db, LoginDto dto) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user is null) return Results.Unauthorized();
            if (!hasher.Verify(dto.Password, user.PasswordHash, user.PasswordSalt)) return Results.Unauthorized();
            var (access, expires) = tokens.CreateAccessToken(user.Id, user.Username, Array.Empty<string>());
            // Simple demo: return only access; refresh omitted for brevity in endpoints (can be added)
            return Results.Ok(new { accessToken = access, expiresUtc = expires });
        });

        return app;
    }
}

public sealed record RegisterDto(string Username, string Email, string Password);
public sealed record LoginDto(string Username, string Password);
