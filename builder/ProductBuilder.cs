using System.Collections.Generic;
using Stock_Server.Models;

namespace Stock_Server.Builder;

public sealed class ProductBuilder
{
    private readonly Product _product = new()
    {
        Id = string.Empty,
        Name = string.Empty,
        Description = null,
        Price = 0m,
        Quantity = 0,
        SupplierId = string.Empty,
        Supplier = null!
    };

    public static ProductBuilder CreateBuilder() => new();

    public ProductBuilder SetId(string id)
    {
        _product.Id = id;
        return this;
    }

    public ProductBuilder SetName(string name)
    {
        _product.Name = name;
        return this;
    }

    public ProductBuilder SetDescription(string? description)
    {
        _product.Description = description;
        return this;
    }

    public ProductBuilder SetPrice(decimal price)
    {
        _product.Price = price;
        return this;
    }

    public ProductBuilder SetQuantity(int quantity)
    {
        _product.Quantity = quantity;
        return this;
    }

    public ProductBuilder SetSupplierId(string supplierId)
    {
        _product.SupplierId = supplierId;
        return this;
    }

    public ProductBuilder SetSupplier(Supplier supplier)
    {
        _product.Supplier = supplier;
        return this;
    }

    public Product Build() => _product;
}