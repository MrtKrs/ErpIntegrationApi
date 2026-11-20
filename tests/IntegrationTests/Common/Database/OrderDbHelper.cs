using Application.Abstractions.Data;
using Dapper;
using IntegrationTests.Common.Fakers;

namespace IntegrationTests.Common.Database;

public sealed record TestOrder(
    int Id,
    string? DocumentId,
    int DocumentType,
    DateTime? OrderDate,
    string? NetAmount,
    string? VatAmount,
    string? TotalAmount,
    int? CustomerId,
    string? CustomerNumber,
    string? CustomerName,
    string? Notes,
    string? SalespersonName,
    string? InvoiceNumber,
    string? ReferencesDocumentNumber);

public sealed class OrderDbHelper(IDbConnectionFactory connectionFactory)
{
    public async Task<TestOrder> SeedOrderAsync(Func<TestOrder, TestOrder>? customize = null)
    {
        var fake = OrderFaker.SeedOrder.Generate();
        var order = customize is null ? fake : customize(fake);

        using var connection = connectionFactory.CreateErpConnection();
        int id = await connection.QuerySingleAsync<int>("""
            INSERT INTO tblSalgsBilag (
                bilagsid, bilagstype, bilagsdato, nettobeløb, moms, bilagsbeløb,
                debitorid, debitornr, debitornavn, bemærkninger, sælgernavn,
                fakturanr, vedrbilagsnr
            )
            OUTPUT INSERTED.id
            VALUES (
                @DocumentId, @DocumentType, @OrderDate, @NetAmount, @VatAmount, @TotalAmount,
                @CustomerId, @CustomerNumber, @CustomerName, @Notes, @SalespersonName,
                @InvoiceNumber, @ReferencesDocumentNumber
            );
            """,
            order);

        return order with { Id = id };
    }
}
