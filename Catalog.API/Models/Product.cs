namespace Catalog.API.Models;

public record Product(
    string Id,
    string Name,
    string Description,
    decimal Price,
    int Stock
);