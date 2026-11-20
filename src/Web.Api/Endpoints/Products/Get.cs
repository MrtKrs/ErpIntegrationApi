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
            [AsParameters] PagedRequest request,
            string? sortColumn,
            string? sortOrder,
            IQueryHandler<GetProductsQuery, PagedList<ProductResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProductsQuery(
                request.Search,
                sortColumn,
                sortOrder,
                request.Page,
                request.PageSize);

            Result<PagedList<ProductResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .HasPermission("products:read")
        .RequireAuthorization();
    }
}
