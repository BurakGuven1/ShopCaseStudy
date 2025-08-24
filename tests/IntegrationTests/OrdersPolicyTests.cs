using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests;

public class OrdersPolicyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public OrdersPolicyTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Get_by_id_should_return_403_for_non_owner()
    {
        var owner = Guid.NewGuid();
        var intruder = Guid.NewGuid();

        Guid orderId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var order = new Domain.Entities.Order(owner);
            order.AddItem(Guid.NewGuid(), "Pencil", 2, 4);
            db.Orders.Add(order);
            await db.SaveChangesAsync();
            orderId = order.Id;
        }

        var client = _factory.CreateClient();
        client.UseJwt(TestAuth.CreateJwt(intruder)); // sahte kullanıcı: intruder

        // NOTE: Versiyonlu route!
        var res = await client.GetAsync($"/api/v1/orders/{orderId}");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Get_by_id_should_return_200_for_owner()
    {
        var owner = Guid.NewGuid();
        Guid orderId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var order = new Domain.Entities.Order(owner);
            order.AddItem(Guid.NewGuid(), "Notebook", 1, 20);
            db.Orders.Add(order);
            await db.SaveChangesAsync();
            orderId = order.Id;
        }

        var client = _factory.CreateClient();
        client.UseJwt(TestAuth.CreateJwt(owner)); // owner’ın token’ı

        // NOTE: Versiyonlu route!
        var res = await client.GetAsync($"/api/v1/orders/{orderId}");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}
