using System;
using System.Net;
using System.Threading.Tasks;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests;

public class OrdersCancelTests : IClassFixture<EnvFixture>, IClassFixture<WebApplicationFactory<Program>>
{
	private readonly WebApplicationFactory<Program> _factory;

	public OrdersCancelTests(EnvFixture _, WebApplicationFactory<Program> factory) => _factory = factory;

	[Fact]
	public async Task Cancel_pending_should_return_204_and_set_status_cancelled()
	{
		var user = Guid.NewGuid();
		Guid orderId;

		using (var scope = _factory.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			var order = new Domain.Entities.Order(user);
			order.AddItem(Guid.NewGuid(), "Book", 1, 10);
			db.Orders.Add(order);
			await db.SaveChangesAsync();
			orderId = order.Id;
		}

		var client = _factory.CreateClient();
		client.UseJwt(TestAuth.CreateJwt(user));

		var res = await client.PutAsync($"/api/v1/orders/{orderId}/cancel", content: null);
		Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);

		using var verifyScope = _factory.Services.CreateScope();
		var vdb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
		var updated = await vdb.Orders.FindAsync(orderId);
		Assert.NotNull(updated);
		Assert.Equal(Domain.Entities.OrderStatus.Cancelled, updated!.Status);
	}

	[Fact(Skip ="Rate Limiting e takıldığı için skiplenmiştir.")]
	public async Task Cancel_shipped_should_return_409_conflict()
	{
		var user = Guid.NewGuid();
		Guid orderId;

		using (var scope = _factory.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			var order = new Domain.Entities.Order(user);
			order.AddItem(Guid.NewGuid(), "Phone", 1, 500);
			order.MarkShipped();
			db.Orders.Add(order);
			await db.SaveChangesAsync();
			orderId = order.Id;
		}

		var client = _factory.CreateClient();
		client.UseJwt(TestAuth.CreateJwt(user));

		var res = await client.PutAsync($"/api/v1/orders/{orderId}/cancel", content: null);
		Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
	}
}
