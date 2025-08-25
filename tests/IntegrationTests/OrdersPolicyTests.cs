using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests;

public class OrdersPolicyTests : IClassFixture<EnvFixture>, IClassFixture<WebApplicationFactory<Program>>
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly HttpClient _client;

	public OrdersPolicyTests(EnvFixture _ /* sırf sırayla çalışsın diye */, WebApplicationFactory<Program> factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
	}

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

		// intruder JWT
		_client.UseJwt(TestAuth.CreateJwt(intruder));

		var res = await _client.GetAsync($"/api/v1/orders/{orderId}");
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

		_client.UseJwt(TestAuth.CreateJwt(owner));

		var res = await _client.GetAsync($"/api/v1/orders/{orderId}");
		Assert.Equal(HttpStatusCode.OK, res.StatusCode);
	}
}
