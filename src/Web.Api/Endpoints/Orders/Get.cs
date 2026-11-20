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
            [AsParameters] PagedRequest request,
            string? sortColumn,
            string? sortOrder,
            IQueryHandler<GetOrdersQuery, PagedList<OrderResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOrdersQuery(
                request.Search,
                sortColumn,
                sortOrder,
                request.Page,
                request.PageSize);

                Result<PagedList<OrderResponse>> result = await handler.Handle(query, cancellationToken);
                return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Orders)
        .HasPermission("orders:read")
        .RequireAuthorization();
    }
}