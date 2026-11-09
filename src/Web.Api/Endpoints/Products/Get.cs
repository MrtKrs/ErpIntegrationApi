using Application.Abstractions.Messaging;
using Application.Abstractions.Pagination;
using Application.Products.Get;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;
internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products", async (
            string? search,
            string? sortColumn,
            string? sortOrder,
            int? page,
            int? pageSize,
            IQueryHandler<GetProductsQuery, PagedList<ProductResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProductsQuery(
                search,
                sortColumn,
                sortOrder,
                page ?? 1,
                pageSize ?? 10);

            Result<PagedList<ProductResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .HasPermission("products:read")
        .RequireAuthorization();
    }
}
