using Application.Abstractions.Persistence;
using Application.Features.Products.Dtos;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _repo;

    public GetProductByIdHandler(IProductRepository repo) => _repo = repo;

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var p = await _repo.GetByIdAsync(request.Id, ct)
                ?? throw new KeyNotFoundException("Product not found");

        return new ProductDto(p.Id, p.Name, p.Price, p.UpdatedAt);
    }
}
