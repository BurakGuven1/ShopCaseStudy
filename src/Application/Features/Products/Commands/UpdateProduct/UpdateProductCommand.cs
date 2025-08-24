using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(Guid Id, string Name, decimal Price) : IRequest<Unit>;
