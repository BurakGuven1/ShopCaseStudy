using FluentValidation;

namespace Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).When(x => !x.UseCursor);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);

        RuleFor(x => x.PriceMin).GreaterThanOrEqualTo(0).When(x => x.PriceMin.HasValue);
        RuleFor(x => x.PriceMax).GreaterThanOrEqualTo(0).When(x => x.PriceMax.HasValue);
        RuleFor(x => x).Must(x => !(x.PriceMin > x.PriceMax))
            .WithMessage("PriceMin cannot be greater than PriceMax")
            .When(x => x.PriceMin.HasValue && x.PriceMax.HasValue);

        RuleFor(x => x.SortDir)
            .Must(v => string.IsNullOrWhiteSpace(v) || v is "asc" or "desc")
            .WithMessage("SortDir must be 'asc' or 'desc'.");

        RuleFor(x => x.SortBy)
            .Must(v => string.IsNullOrWhiteSpace(v) || v is "name" or "price" or "updatedAt")
            .WithMessage("SortBy must be one of: name, price, updatedAt.");
    }
}
