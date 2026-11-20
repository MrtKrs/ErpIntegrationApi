using Application.Abstractions.Pagination;

namespace Application.UnitTests.Abstractions.Pagination;

public sealed class PagedQueryEnxtensionsTests
{
    private sealed record TestPagedQuery(string? Search, int Page, int PageSize) : IPagedQuery;

    [Theory]
    [InlineData(1,10,0 )]
    [InlineData(2,10,10)]
    [InlineData(3,25,50)]
    public void GetOffSet_ShouldCalculateCorrectOffset(int page, int pageSize, int expectedOffSet)
    {
        var query = new TestPagedQuery(Search: null, Page:page, PageSize: pageSize);

        query.GetOffSet().ShouldBe(expectedOffSet);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToSqlLikePattern_ShouldReturnNull_WhenSearchIsNullOrWhiteSpace(string? search)
    {
        var query = new TestPagedQuery(Search: search, Page:1, PageSize: 10);

        query.ToSqlLikePattern().ShouldBeNull();
    }

    [Theory]
    [InlineData("Acme", "%Acme%")]
    [InlineData("  Acme    ", "%Acme%")]
    [InlineData("50% off", "%50[%] off%")]
    [InlineData("item_1", "%item[_]1%")]
    [InlineData("tag[A]", "%tag[[]A]%")]
    public void ToSqlLikePattern_ShouldTrimAndEscapeWildcards(string search, string expectedPattern)
    {
        var query = new TestPagedQuery(Search: search, Page:1, PageSize: 10);

        query.ToSqlLikePattern().ShouldBe(expectedPattern);
    }
}

