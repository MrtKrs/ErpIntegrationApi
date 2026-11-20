using Application.Abstractions.Messaging;
using Application.Abstractions.Pagination;

namespace Application.Products.Get;

public sealed record GetProductsQuery(
    string? Search,
    string? SortColumn,
    string? SortOrder,
    int Page = 1,
    int PageSize = 1) : IQuery<PagedList<ProductResponse>>, IPagedQuery;
