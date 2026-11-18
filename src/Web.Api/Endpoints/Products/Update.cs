using Application.Abstractions.Messaging;
using Application.Products.Update;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

internal sealed class Update : IEndpoint
{
    public sealed record Request(string Sku, string Name, string Price);
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("products/{id:int}", async (
            int id,
            Request request,
            ICommandHandler<UpdateProductCommand> handler,
            CancellationToken cancellationToken ) =>
        {
            var command = new UpdateProductCommand(id, request.Sku, request.Name, request.Price);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .HasPermission("products:read")
        .RequireAuthorization();
    }
}