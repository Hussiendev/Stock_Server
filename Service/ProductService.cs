using Microsoft.Extensions.Logging;
using Stock_Server.Mapper;
using Stock_Server.Models;
using Stock_Server.Repository;
using Stock_Server.Util.Exceptions;

namespace Stock_Server.Service;

public class ProductService : IProductService
{
    private readonly IRepository<Product> _repository;
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly JSONProductMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IRepository<Product> repository, IRepository<Supplier> supplierRepository, JSONProductMapper mapper, ILogger<ProductService> logger)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<JSONProduct>> GetAllProductsAsync()
    {
        var products = await _repository.GetAllAsync();
        return products.Select(p => _mapper.ReverseMap(p));
    }

    public async Task<JSONProduct> GetProductByNameAsync(string name)
    {
        var product = await _repository.GetByNameAsync(name);
        if (product == null)
            throw new ItemNotFoundException($"Product with name '{name}' not found");
        return _mapper.ReverseMap(product);
    }

    public async Task<JSONProduct> CreateProductAsync(JSONProduct jsonProduct)
    {
        await ValidateProductAsync(jsonProduct);
        var resolvedSupplierId = await ResolveSupplierIdAsync(jsonProduct.SupplierName);
        var product = _mapper.Map(jsonProduct);
        product.SupplierId = resolvedSupplierId;
        var created = await _repository.CreateAsync(product);
        // Get supplier name for response
        var supplier = await _supplierRepository.GetByIdAsync(resolvedSupplierId);
        return new JSONProduct
        {
            Id = created.Id,
            Name = created.Name,
            Description = created.Description,
            Price = created.Price,
            Quantity = created.Quantity,
            SupplierName = supplier?.Name ?? string.Empty
        };
    }

    public async Task UpdateProductAsync(string name, UpdateJSONProduct updateRequest)
    {
        var existing = await _repository.GetByNameAsync(name);
        if (existing == null)
            throw new ItemNotFoundException($"Product with name '{name}' not found");

        if (!string.IsNullOrWhiteSpace(updateRequest.Name))
            existing.Name = updateRequest.Name;
        if (updateRequest.Description != null)
            existing.Description = updateRequest.Description;
        if (updateRequest.Price.HasValue)
            existing.Price = updateRequest.Price.Value;
        if (updateRequest.Quantity.HasValue)
            existing.Quantity = updateRequest.Quantity.Value;
        if (!string.IsNullOrWhiteSpace(updateRequest.SupplierName))
        {
            var newSupplierId = await ResolveSupplierIdAsync(updateRequest.SupplierName);
            existing.SupplierId = newSupplierId;
        }
        await _repository.UpdateByNameAsync(name,existing);
    }

    public async Task DeleteProductAsync(string id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<decimal> GetCapital(string name)
    {
        var product = await _repository.GetByNameAsync(name);
        if (product == null)
            throw new ItemNotFoundException($"Product with name '{name}' not found");
        return product.Price * product.Quantity;
    }

    private async Task ValidateProductAsync(JSONProduct product, string? currentProductName = null)
    {
        if (string.IsNullOrWhiteSpace(product.Name)){
            throw new BadRequestException("Product name is required.");

        }
         if (string.IsNullOrWhiteSpace(product.Description)){
            throw new BadRequestException("Product description is required.");

        }
        if (product.Price < 0){
            throw new BadRequestException("Product price cannot be negative.");

        }
        if (product.Quantity < 0){
            throw new BadRequestException("Product quantity cannot be negative.");
        }
        if (string.IsNullOrWhiteSpace(product.SupplierName)){
            throw new BadRequestException("Supplier name is required.");
        }
        // Check supplier exists efficiently
        var supplierExists = await _supplierRepository.ExistsByNameAsync(product.SupplierName);
        if (!supplierExists){
            throw new BadRequestException($"Supplier '{product.SupplierName}' does not exist.");
        }
        // Check product name uniqueness
        var productExists = await _repository.ExistsByNameAsync(product.Name, currentProductName);
        if (productExists){
            throw new BadRequestException($"Product '{product.Name}' already exists.");
        }
    }

    private async Task<string> ResolveSupplierIdAsync(string supplierName)
    {
        var supplier = await _supplierRepository.GetByNameAsync(supplierName);
        if (supplier == null){
            throw new BadRequestException($"Supplier '{supplierName}' does not exist.");
            }

        return supplier.Id;
    }
}
