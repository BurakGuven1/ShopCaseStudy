using Application.Abstractions;
using Application.Abstractions.Persistence;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly IProductRepository _repo;
    private readonly ICacheService _cache;

    public UpdateProductHandler(IProductRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var p = await _repo.GetByIdAsync(request.Id, ct)
                ?? throw new KeyNotFoundException("Product not found");

        p.Update(request.Name, request.Price);

        await _repo.UpdateAsync(p, ct);
        await _cache.IncrementAsync("products:version", ct);

        return Unit.Value;
    }
}
