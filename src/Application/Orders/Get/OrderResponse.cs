namespace Application.Orders.Get;

public sealed record OrderResponse(
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
