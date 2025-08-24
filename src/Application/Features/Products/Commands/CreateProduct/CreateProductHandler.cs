using Application.Abstractions;
using Application.Abstractions.Persistence;
using Domain.Entities;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _products;
    private readonly ICacheService _cache;

    public CreateProductHandler(IProductRepository products, ICacheService cache)
    {
        _products = products;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var entity = new Product
        {
            Name = request.Name,
            Price = request.Price
        };

        await _products.AddAsync(entity, ct);

        await _cache.IncrementAsync("products:version", ct);

        return entity.Id;
    }
}
