namespace Stock_Server.Mapper;

public record JSONProduct
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int Quantity { get; init; }
    public string SupplierName { get; init; } = string.Empty;
}
public record UpdateJSONProduct
{
    public string ?Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal ?Price { get; init; }
    public int ?Quantity { get; init; }
    public string ?SupplierName { get; init; } = string.Empty;
}