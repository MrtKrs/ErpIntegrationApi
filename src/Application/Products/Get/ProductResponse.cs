namespace Application.Products.Get;

public sealed record ProductResponse(
    int Id,
    string Sku,
    string Name,
    string Price
    );