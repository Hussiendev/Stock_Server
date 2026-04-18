using Stock_Server.Builder;
using Stock_Server.Models;
using Stock_Server.Util;

namespace Stock_Server.Mapper;

public class JSONSupplierMapper : IMapper<JSONSupplier, Supplier>
{
    public Supplier Map(JSONSupplier input) => SupplierBuilder.CreateBuilder()
        .SetId(IdGenerator.Generate("supplier"))
        .SetName(input.Name)
        .Build();

    public JSONSupplier ReverseMap(Supplier input) => new()
    {
        Id = input.Id,
        Name = input.Name
    };
}