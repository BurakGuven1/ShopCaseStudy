using System;

public sealed class EnvFixture
{
	public EnvFixture()
	{
		// Program.cs bunları okuyor
		Environment.SetEnvironmentVariable("MEDIATR_LICENSE_KEY", "CI_OR_LOCAL_TEST_KEY");
		Environment.SetEnvironmentVariable("Jwt__Issuer", "Shop.Api");
		Environment.SetEnvironmentVariable("Jwt__Audience", "Shop.Api.Client");
		Environment.SetEnvironmentVariable("Jwt__Key", "INTEGRATION_TESTS_ONLY_32_CHARS_MINIMUM_123456"); // 32+ char

		// Testlerde rate limit kapansın (varsa)
		Environment.SetEnvironmentVariable("DISABLE_RATE_LIMITING", "true");

		// Local integration için DB/Redis (CI’da zaten services bölümünde veriyorsun)
		Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Host=127.0.0.1;Port=5432;Database=shop_db;Username=user;Password=password");
		Environment.SetEnvironmentVariable("ConnectionStrings__Redis", "127.0.0.1:6379,abortConnect=false");

		// İstiyorsan:
		Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
	}
}
