using Application.Abstractions.Messaging;
using Application.Products.GetById;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/{id:int}", async (
            int id, 
            IQueryHandler<GetProductByIdQuery, ProductResponse> handler, 
            CancellationToken cancellationToken) =>
        {
            var query = new GetProductByIdQuery(id);
            Result<ProductResponse> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .HasPermission("products:read")
        .RequireAuthorization();
    }
}
