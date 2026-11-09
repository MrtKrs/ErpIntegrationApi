using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Net.Http.Headers;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Pagination;
using Dapper;
using SharedKernel;

namespace Application.Products.Get;

internal sealed class GetProductsQueryHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetProductsQuery, PagedList<ProductResponse>>
{
    public async Task<Result<PagedList<ProductResponse>>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();

        int offset = (query.Page -1) * query.PageSize;

        //TODO: Search, SortColumns, SorOrder missing:
        const string sql = @"
        SELECT COUNT(*) FROM tblVarer;

        SELECT
            id AS Id,
            varenummer AS Sku,
            varenavn AS Name,
            standardpris AS Price
        FROM tblVarer
        ORDER BY id ASC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        using var multipleResults = await connection.QueryMultipleAsync(
            new CommandDefinition( sql, new { 
            Offset = offset,
            query.PageSize},
            cancellationToken : cancellationToken));
        //new CommandDefinition(sql, new {query.Search, query.SortColumn, query.SortOrder, offset, query.PageSize},

        int totalCount = await multipleResults.ReadFirstAsync<int>();
        var products = (await multipleResults.ReadAsync<ProductResponse>()).ToList();

        var productsPaged = new PagedList<ProductResponse>(products, totalCount, query.Page, query.PageSize);
        
        return Result.Success(productsPaged);
    }
}