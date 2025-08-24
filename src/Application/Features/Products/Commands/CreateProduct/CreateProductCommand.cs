using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(string Name, decimal Price) : IRequest<Guid>;
