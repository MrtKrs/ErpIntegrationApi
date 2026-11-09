using System;
using FluentValidation;

namespace Application.Products.Get;

internal sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(c => c.Page)
            .GreaterThan(0);
            //.WithMessage("The page must be greater than zero.");
        RuleFor(c => c.PageSize)
            .GreaterThan(0);
            //.WithMessage("The PageSize must be greater than zero.");
    }
}