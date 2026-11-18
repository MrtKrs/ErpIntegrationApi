using Application.Abstractions.Messaging;

namespace Application.Products.Update;

public sealed record UpdateProductCommand(
    int Id,
    string Sku,
    string Name,
    string Price
    ) : ICommand;