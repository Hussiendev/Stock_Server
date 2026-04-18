using System.Collections.Generic;
using System.Linq;
using Stock_Server.Models;

namespace Stock_Server.Builder;

public sealed class SupplierBuilder
{
    private readonly Supplier _supplier = new()
    {
        Id = string.Empty,
        Name = string.Empty,
        Products = new List<Product>()
    };

    public static SupplierBuilder CreateBuilder() => new();

    public SupplierBuilder SetId(string id)
    {
        _supplier.Id = id;
        return this;
    }

    public SupplierBuilder SetName(string name)
    {
        _supplier.Name = name;
        return this;
    }

    public SupplierBuilder SetProducts(IEnumerable<Product> products)
    {
        _supplier.Products = products.ToList();
        return this;
    }

    public SupplierBuilder AddProduct(Product product)
    {
        _supplier.Products.Add(product);
        return this;
    }

    public Supplier Build() => _supplier;
}