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
            id AS Id,
            varenummer AS Sku,
            varenavn AS Name,
            standardpris AS Price
        FROM tblVarer
        WHERE Id = @ProductId";

        ProductResponse? product = await connection.QuerySingleOrDefaultAsync<ProductResponse>(
            new CommandDefinition(sql, new { query.ProductId}, cancellationToken: cancellationToken));
        
        if(product is null)
        {
            return Result.Failure<ProductResponse>(ProductErrors.NotFound(query.ProductId));
        }

        return Result.Success(product);
    }
}
