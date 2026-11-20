using System;
using System.Data;
using Application.Abstractions.Pagination;
using FluentValidation;

namespace Application.Products.Get;

internal sealed class GetProductsQueryValidator : PagedQueryValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
    }
}