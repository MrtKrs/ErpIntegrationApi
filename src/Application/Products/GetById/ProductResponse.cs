namespace Application.Products.GetById;

public sealed record ProductResponse(
    int Id,
    string Sku,
    string Name,
    string Price
    );