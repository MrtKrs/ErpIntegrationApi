using System;
using FluentValidation;

namespace Application.Products.GetById;

internal sealed class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(c => c.ProductId)
            .GreaterThan(0)
            .WithMessage("The product ID must be greater than zero.");
    }
}
