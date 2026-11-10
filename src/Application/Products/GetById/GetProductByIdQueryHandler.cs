using System;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Products;
using SharedKernel;

namespace Application.Products.GetById;

internal sealed class GetProductByIdQueryHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetProductByIdQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
        SELECT 
            v.id AS Id,
            v.varenummer AS Sku,
            v.varenavn AS Name,
            v.standardpris AS Price,
            l.id AS StockId,
            l.beholdning AS StockQuantity,
            l.disponibel AS AvailableQuantity
        FROM tblVarer v
        LEFT JOIN tblLager l ON v.id = l.vareid
        WHERE v.id = @ProductId;";

        var products = await connection.QueryAsync<VareDto, LagerDto, ProductResponse> (
            sql,
            (vare, lager) =>
            {
                return new ProductResponse (
                    vare.Id,
                    vare.Sku ?? string.Empty,
                    vare.Name ?? string.Empty,
                    vare.Price ?? string.Empty,
                    lager?.StockQuantity ?? 0,
                    lager?.AvailableQuantity ?? 0
                );
            },
            new { query.ProductId},
            splitOn: "StockId"
        );
        var product = products.FirstOrDefault();
        
        if(product is null)
        {
            return Result.Failure<ProductResponse>(ProductErrors.NotFound(query.ProductId));
        }

        return Result.Success(product);
    }

    private sealed record VareDto( 
        int Id,
        string? Sku, 
        string? Name, 
        string? Price
        );

    private sealed record LagerDto(
        int StockId,
        int StockQuantity,
        int AvailableQuantity
    );
}
