using Application.Abstractions.Pagination;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Abstractions.Pagination;
public sealed class PagedQueryValidatorTests
{
    private sealed record TestPagedQuery(string? Search, int Page, int PageSize) : IPagedQuery;
    private sealed class TestPagedQueryValidator : PagedQueryValidator<TestPagedQuery>;
    private readonly TestPagedQueryValidator _validator = new();
    
    [Theory]
    [InlineData(1,1)]
    [InlineData(1,100)]
    [InlineData(5,100)]
    public void ShouldNotHaveErrors_WhenPageAndSizeAreValid(int page, int pageSize)
    {
        var query = new TestPagedQuery(Search: null, Page: page, PageSize: pageSize);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ShouldHaveError_WhenPageIsLessThanOne(int page)
    {
        var query = new TestPagedQuery(Search: null, Page: page, PageSize: 10);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void ShouldHaveError_WhenPageSizeIsOutOfRange(int pageSize)
    {
        var query = new TestPagedQuery(Search: null, Page: 1, PageSize: pageSize);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }
}
