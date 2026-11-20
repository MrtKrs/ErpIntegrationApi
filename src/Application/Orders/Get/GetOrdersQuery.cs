using Application.Abstractions.Messaging;
using Application.Abstractions.Pagination;

namespace Application.Orders.Get;

public sealed record GetOrdersQuery(
    string? Search,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IQuery<PagedList<OrderResponse>>;
