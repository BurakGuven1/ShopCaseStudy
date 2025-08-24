namespace Application.Features.Products.Dtos;

public sealed record ProductDto(Guid Id, string Name, decimal Price, DateTimeOffset UpdatedAt);
