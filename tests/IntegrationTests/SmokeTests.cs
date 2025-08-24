using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Threading.Tasks;

namespace IntegrationTests;

public class SmokeTests: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public SmokeTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Health_should_return_200()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}
