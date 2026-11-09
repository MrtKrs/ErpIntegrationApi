using System;
using System.Data;
using FluentValidation;

namespace Application.Products.Get;

internal sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(c => c.Page)
            .GreaterThan(0);
        RuleFor(c => c.PageSize)
            .GreaterThan(0);
        RuleFor(c=> c.Search)
            .MinimumLength(3)
            .When(c=> !string.IsNullOrWhiteSpace(c.Search));
    }
}