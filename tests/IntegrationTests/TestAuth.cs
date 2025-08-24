using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IntegrationTests;

internal static class TestAuth
{
    private const string Issuer = "Shop.Api";
    private const string Audience = "Shop.Api.Client";
    private const string Key = "CHANGE_ME_SUPER_SECRET_KEY_32CHARS_MINIMUM";

    internal static string CreateJwt(Guid userId, bool admin = false, IEnumerable<Claim>? extraClaims = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        if (admin) claims.Add(new(ClaimTypes.Role, "Admin"));
        if (extraClaims is not null) claims.AddRange(extraClaims);

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
