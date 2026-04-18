using Stock_Server.Builder;
using Stock_Server.Models;
using Stock_Server.Util;

namespace Stock_Server.Mapper;

public class JSONProductMapper : IMapper<JSONProduct, Product>
{
    public Product Map(JSONProduct input) => ProductBuilder.CreateBuilder()
        .SetId(IdGenerator.Generate("product"))
        .SetName(input.Name)
        .SetDescription(input.Description)
        .SetPrice(input.Price)
        .SetQuantity(input.Quantity)
        .Build();

    public JSONProduct ReverseMap(Product input) => new()
    {
        Id = input.Id,
        Name = input.Name,
        Description = input.Description,
        Price = input.Price,
        Quantity = input.Quantity,
        SupplierName = input.Supplier?.Name ?? string.Empty
    };
}