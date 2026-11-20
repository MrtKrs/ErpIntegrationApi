using Application.Abstractions.Messaging;
using Application.Abstractions.Pagination;
using Application.Orders.Get;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Orders;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders", async (
            string? search,
            string? sortColumn,
            string? sortOrder,
            int? page,
            int? pageSize,
            IQueryHandler<GetOrdersQuery, PagedList<OrderResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOrdersQuery(
                search,
                sortColumn,
                sortOrder,
                page ?? 1,
                pageSize ?? 10);

                Result<PagedList<OrderResponse>> result = await handler.Handle(query, cancellationToken);
                return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Orders)
        .HasPermission("orders:read")
        .RequireAuthorization();
    }
}