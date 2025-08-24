using Application.Features.Products.Dtos;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;
