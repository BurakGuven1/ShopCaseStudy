using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public static class TestAuth
{
	public static string CreateJwt(Guid userId, string? role = null)
	{
		var issuer = Environment.GetEnvironmentVariable("Jwt__Issuer") ?? "Shop.Api";
		var audience = Environment.GetEnvironmentVariable("Jwt__Audience") ?? "Shop.Api.Client";
		var key = Environment.GetEnvironmentVariable("Jwt__Key") ?? "INTEGRATION_TESTS_ONLY_32_CHARS_MINIMUM_123456";

		var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
			new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		var handler = new JwtSecurityTokenHandler();
		var token = new JwtSecurityToken(
			issuer: issuer,
			audience: audience,
			claims: role is null ? claims : Append(claims, new Claim(ClaimTypes.Role, role)),
			expires: DateTime.UtcNow.AddHours(1),
			signingCredentials: creds);

		return handler.WriteToken(token);
	}

	private static T[] Append<T>(T[] source, T item)
	{
		var arr = new T[source.Length + 1];
		Array.Copy(source, arr, source.Length);
		arr[^1] = item;
		return arr;
	}
}

public static class HttpClientJwtExtensions
{
	public static void UseJwt(this System.Net.Http.HttpClient client, string jwt)
		=> client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
}
