using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Application.Common;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests;

public class ProductsEtagTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public ProductsEtagTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Products_list_should_return_304_when_if_none_match_matches()
    {
        // Arrange: DB’ye bir-iki ürün ekle
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Products.Add(new Domain.Entities.Product { Name = "Keyboard", Price = 100 });
            db.Products.Add(new Domain.Entities.Product { Name = "Mouse", Price = 50 });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        // NOTE: Versiyonlu route!
        var first = await client.GetAsync("/api/v1/products?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.NotNull(first.Headers.ETag);

        var etag = first.Headers.ETag!.Tag?.Trim('"');
        Assert.False(string.IsNullOrWhiteSpace(etag));

        // Aynı sorguyu If-None-Match ile vur → 304 beklenir
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/products?page=1&pageSize=10");
        req.Headers.TryAddWithoutValidation("If-None-Match", $"\"{etag}\"");

        var second = await client.SendAsync(req);
        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
    }
}
