using Application.Common.Specifications;
using Domain.Entities;

namespace Application.Features.Products.Queries.GetProducts.Specs;

public sealed class ProductFilteredOnlySpec : BaseSpecification<Product>
{
    public ProductFilteredOnlySpec(
        string? nameContains,
        decimal? priceMin,
        decimal? priceMax)
    {
        Criteria = p =>
            (string.IsNullOrEmpty(nameContains) || p.Name.Contains(nameContains)) &&
            (!priceMin.HasValue || p.Price >= priceMin.Value) &&
            (!priceMax.HasValue || p.Price <= priceMax.Value);
    }
}
