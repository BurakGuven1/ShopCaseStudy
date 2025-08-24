using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Abstractions.Security;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SymmetricSecurityKey _key;

    public JwtTokenGenerator(string issuer, string audience, SymmetricSecurityKey key)
    {
        _issuer = issuer;
        _audience = audience;
        _key = key;
    }

    public string CreateToken(Guid userId, string userName, string email, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, userName ?? email ?? string.Empty),
            new Claim(ClaimTypes.Email, email ?? string.Empty),
            new Claim(ClaimTypes.Role, "User")
        };

        if (roles != null)
        {
            foreach (var r in roles.Distinct())
                claims.Add(new Claim(ClaimTypes.Role, r));
        }

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
