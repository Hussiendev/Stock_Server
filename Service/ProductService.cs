using Microsoft.Extensions.Logging;
using Stock_Server.Mapper;
using Stock_Server.Models;
using Stock_Server.Repository;
using Stock_Server.Util.Exceptions;

namespace Stock_Server.Service;

public class ProductService
{
    private readonly IRepository<Product> _repository;
    private readonly SupplierRepository _supplierRepository;
    private readonly JSONProductMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IRepository<Product> repository, SupplierRepository supplierRepository, JSONProductMapper mapper, ILogger<ProductService> logger)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<JSONProduct>> GetAllProductsAsync()
    {
        _logger.LogInformation("Retrieving all products");
        var products = await _repository.GetAllAsync();
        var result = products.Select(p => _mapper.ReverseMap(p));
        _logger.LogInformation("Retrieved {Count} products", products.Count());
        return result;
    }

    public async Task<JSONProduct> GetProductByNameAsync(string name)
    {
        _logger.LogInformation("Retrieving product with name {Name}", name);
        var product = await _repository.GetByNameAsync(name);
        var result = _mapper.ReverseMap(product);
        _logger.LogInformation("Retrieved product with name {Name}", name);
        return result;
    }

 public async Task<JSONProduct> CreateProductAsync(JSONProduct jsonProduct)
{
    _logger.LogInformation("Validating and creating product with name {Name}", jsonProduct.Name);
    await ValidateProductAsync(jsonProduct);

    var resolvedSupplierId = await ResolveSupplierIdAsync(jsonProduct);
    var product = _mapper.Map(jsonProduct);
    product.SupplierId = resolvedSupplierId;
    var createdProduct = await _repository.CreateAsync(product);

    _logger.LogInformation("Created product with ID {Id}", createdProduct.Id);
    
    // Get the supplier name for the response
    var supplierName = await _supplierRepository.GetSupplierNameByIdAsync(product.SupplierId);
    
    return new JSONProduct
    {
        Id = createdProduct.Id,
        Name = createdProduct.Name,
        Description = createdProduct.Description,
        Price = createdProduct.Price,
        Quantity = createdProduct.Quantity,
        SupplierName = supplierName
    };
}
 public async Task UpdateProductAsync(string name, UpdateJSONProduct updateRequest)
{
    _logger.LogInformation("Updating product with name {Name}", name);
    
    // Get existing product from database (includes Supplier)
    var existingProduct = await _repository.GetByNameAsync(name);
    
    // Update only the fields that are provided (non-null)
    if (!string.IsNullOrWhiteSpace(updateRequest.Name))
        existingProduct.Name = updateRequest.Name;
    
    if (updateRequest.Description != null)
        existingProduct.Description = updateRequest.Description;
    
    if (updateRequest.Price.HasValue)
        existingProduct.Price = updateRequest.Price.Value;
    
    if (updateRequest.Quantity.HasValue)
        existingProduct.Quantity = updateRequest.Quantity.Value;
    
    if (!string.IsNullOrWhiteSpace(updateRequest.SupplierName))
    {
        // Resolve the new supplier ID
        var newSupplierId = await ResolveSupplierIdAsync(new JSONProduct { SupplierName = updateRequest.SupplierName });
        existingProduct.SupplierId = newSupplierId;
    }
    
    // Save changes
    await _repository.UpdateAsync(existingProduct);
    _logger.LogInformation("Updated product with name {Name}", name);
}
    public async Task DeleteProductAsync(string id)
    {
        _logger.LogInformation("Deleting product with ID {Id}", id);
        await _repository.DeleteAsync(id);
        _logger.LogInformation("Deleted product with ID {Id}", id);
    }
    public async Task<decimal> GetCapital(string name)
{
    _logger.LogInformation("Calculating capital for product with name {Name}", name);
    var product = await _repository.GetByNameAsync(name);
    var capital = product.Price * product.Quantity;
    _logger.LogInformation("Calculated capital for product with name {Name}: {Capital}", name, capital);
    return capital;
}
    private async Task ValidateProductAsync(JSONProduct product, string? currentProductName = null)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            throw new BadRequestException("Product name is required and cannot be empty.");
        }

        if (product.Price < 0)
        {
            throw new BadRequestException("Product price cannot be negative.");
        }

        if (product.Quantity < 0)
        {
            throw new BadRequestException("Product quantity cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(product.SupplierName))
        {
            throw new BadRequestException("Supplier name is required.");
        }

        // Check if supplier exists
        var suppliers = await _supplierRepository.GetAllAsync();
        if (!suppliers.Any(s => s.Name.Equals(product.SupplierName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BadRequestException($"Supplier with name '{product.SupplierName}' does not exist. Please add the supplier first.");
        }

        // Optional: Check if product with same name already exists, excluding current product when updating.
        var allProducts = await _repository.GetAllAsync();
        if (allProducts.Any(p => p.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase)
                                 && !string.Equals(p.Name, currentProductName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BadRequestException($"Product with name '{product.Name}' already exists.");
        }
    }

    private async Task<string> ResolveSupplierIdAsync(JSONProduct product)
    {
        var suppliers = await _supplierRepository.GetAllAsync();
        var supplier = suppliers.FirstOrDefault(s => s.Name.Equals(product.SupplierName, StringComparison.OrdinalIgnoreCase));
        if (supplier == null)
        {
            throw new BadRequestException($"Supplier with name '{product.SupplierName}' does not exist.");
        }

        return supplier.Id;
    }
}
