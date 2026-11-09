using Application.Abstractions.Messaging;
using Application.Abstractions.Pagination;

namespace Application.Products.Get;

public sealed record GetProductsQuery(
    //Guid UserId,
    string? Search,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IQuery<PagedList<ProductResponse>>;
