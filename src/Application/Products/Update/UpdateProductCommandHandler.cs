using System;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Products;
using SharedKernel;

namespace Application.Products.Update;

public class UpdateProductCommandHandler( IDbConnectionFactory connectionFactory )
    : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE tblVarer
            SET
                varenummer = @Sku,
                varenavn = @Name,
                standardpris = @Price
            WHERE id = @Id;";

        int rowsAffected = await connection.ExecuteAsync(sql, command);

        if(rowsAffected == 0)
        {
            return Result.Failure(ProductErrors.NotFound(command.Id));
        }

        return Result.Success();
    }
}
