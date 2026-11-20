using Application.Abstractions.Messaging;
using Application.Abstractions.Pagination;

namespace Application.Orders.Get;

public sealed record GetOrdersQuery(
    string? Search,
    string? SortColumn,
    string? SortOrder,
    int Page = 1,
    int PageSize = 1) : IQuery<PagedList<OrderResponse>>, IPagedQuery;
