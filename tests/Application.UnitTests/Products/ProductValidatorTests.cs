using Application.Products.Get;
using Domain.Products;
using FluentValidation.TestHelper;

namespace  Application.UnitTests.Products;

public sealed class ProductValidatorTests
{
    private readonly GetProductsQueryValidator _getValidator = new();

    [Fact]
    public void GetValidator_Should_HaveError_WhenPageIsZeroOrNegative()
    {
        var query = new GetProductsQuery(Page: 0, PageSize: 10, Search: null, SortColumn: null, SortOrder: null);

        var result = _getValidator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.Page);
    }

    [Fact]
    public void GetValidator_Should_HaveError_WhenSearchIsTooShort()
    {
        var query = new GetProductsQuery(Page: 1, PageSize: 10, Search: "ab", SortColumn: null, SortOrder: null);

        var result = _getValidator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.Search);
    }

    [Fact]
    public void GetValidator_Should_Pass_WhenQueryIsValid()
    {
        var query = new GetProductsQuery(Page: 1, PageSize: 10, Search: "Valid", SortColumn: null, SortOrder: null);

        var result = _getValidator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}