using Application.Abstractions;
using Application.Abstractions.Persistence;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductHandler : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly IProductRepository _products;
    private readonly ICacheService _cache;

    public DeleteProductHandler(IProductRepository products, ICacheService cache)
    {
        _products = products;
        _cache = cache;
    }

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var entity = await _products.GetByIdAsync(request.Id, ct)
                    ?? throw new KeyNotFoundException("Product not found");

        await _products.DeleteAsync(entity, ct);

        await _cache.IncrementAsync("products:version", ct);

        return Unit.Value;
    }
}
