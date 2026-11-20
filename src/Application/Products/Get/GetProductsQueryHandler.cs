using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Net.Http.Headers;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Pagination;
using Dapper;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Products.Get;

internal sealed class GetProductsQueryHandler(IDbConnectionFactory connectionFactory )
    : IQueryHandler<GetProductsQuery, PagedList<ProductResponse>>
{
    public async Task<Result<PagedList<ProductResponse>>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateErpConnection();

        //TODO: SortColumns, SorOrder missing:
        string countSql = "SELECT COUNT (*) FROM tblVarer v WHERE 1=1";
        string resultSql = @"
        SELECT
            v.id AS Id,
            v.varenummer AS Sku,
            v.varenavn AS Name,
            v.standardpris AS Price
        FROM tblVarer v
        WHERE 1=1
        ";

        var parameters = new DynamicParameters();
        parameters.Add("Offset", query.GetOffSet());
        parameters.Add("PageSize", query.PageSize);

        string? searchPattern = query.ToSqlLikePattern();
        if(searchPattern is not null)
        {
            parameters.Add("SearchPattern", searchPattern);

            string searchFilter = " AND ( v.varenavn LIKE @SearchPattern OR v.varenummer LIKE @SearchPattern )";
            countSql += searchFilter;
            resultSql += searchFilter;
        }
        resultSql += "\n ORDER BY v.id ASC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        string sql = $"{countSql};\n{resultSql}";
        
        using var multipleResults = await connection.QueryMultipleAsync(
            new CommandDefinition( sql, 
            parameters,
            cancellationToken : cancellationToken));

        int totalCount = await multipleResults.ReadFirstAsync<int>();
        var products = (await multipleResults.ReadAsync<ProductResponse>()).ToList();

        var productsPaged = new PagedList<ProductResponse>(products, totalCount, query.Page, query.PageSize);
        
        return Result.Success(productsPaged);
    }
}