using System;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Pagination;
using Dapper;
using SharedKernel;

namespace Application.Orders.Get;

public class GetOrdersQueryHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetOrdersQuery, PagedList<OrderResponse>>
{
    public async Task<Result<PagedList<OrderResponse>>> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateErpConnection();

        //TODO: SortColumns, SorOrder
        string countSql = "SELECT COUNT (*) FROM [tblSalgsBilag] AS sb WHERE 1=1";
        string resultSql = """
        SELECT
            sb.[id] AS Id,
            sb.[bilagsid] AS DocumentId, 
            sb.[bilagstype] AS DocumentType, 
            sb.[bilagsdato] AS OrderDate, 
            sb.[nettobeløb] AS NetAmount, 
            sb.[moms] AS VatAmount, 
            sb.[bilagsbeløb] AS TotalAmount,
            sb.[debitorid] AS CustomerId, 
            sb.[debitornr] AS CustomerNumber, 
            sb.[debitornavn] AS CustomerName, 
            sb.[bemærkninger] AS Notes, 
            sb.[sælgernavn] AS SalespersonName,
            sb.[fakturanr] AS InvoiceNumber, 
            sb.[vedrbilagsnr] AS ReferencesDocumentNumber
        FROM [tblSalgsBilag] AS sb
        WHERE 1=1
        """;

        var parameters = new DynamicParameters();
        parameters.Add("Offset", query.GetOffSet());
        parameters.Add("PageSize", query.PageSize);

        string? searchPattern = query.ToSqlLikePattern();
        if(searchPattern is not null)
        {
            parameters.Add("SearchPattern", searchPattern);

            string searchFilter = " AND ( sb.[bilagsid] LIKE @SearchPattern OR sb.[debitornavn] LIKE @SearchPattern )";
            countSql += searchFilter;
            resultSql += searchFilter;
        }
        resultSql += "\n ORDER BY sb.[id] ASC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        string sql = $"{countSql};\n{resultSql}";

        using var multipleResults = await connection.QueryMultipleAsync(
            new CommandDefinition(sql,
            parameters,
            cancellationToken: cancellationToken));

        int totalCount = await multipleResults.ReadFirstAsync<int>();
        var orders = (await multipleResults.ReadAsync<OrderResponse>()).ToList();

        var ordersPaged = new PagedList<OrderResponse>(orders, totalCount, query.Page, query.PageSize);

        return Result.Success(ordersPaged);
    }
}
