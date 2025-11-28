using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Quasar.Identity.Web;

namespace BEAM.App.Services;

public interface IDeviceTokenService
{
    string GenerateToken(Guid deviceId);
}

public class DeviceTokenService : IDeviceTokenService
{
    private readonly JwtOptions _options;

    public DeviceTokenService(JwtOptions options)
    {
        _options = options;
    }

    public string GenerateToken(Guid deviceId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_options.Key);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, deviceId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("type", "device") // Custom claim to distinguish devices from users
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddYears(1), // Long-lived token for devices
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
