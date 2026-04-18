using Microsoft.Extensions.Logging;
using Stock_Server.Mapper;
using Stock_Server.Models;
using Stock_Server.Repository;
using Stock_Server.Util.Exceptions;

namespace Stock_Server.Service;

public class SupplierService
{
    private readonly IRepository<Supplier> _repository;
    private readonly JSONSupplierMapper _mapper;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(IRepository<Supplier> repository, JSONSupplierMapper mapper, ILogger<SupplierService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<JSONSupplier>> GetAllSuppliersAsync()
    {
        _logger.LogInformation("Retrieving all suppliers");
        var suppliers = await _repository.GetAllAsync();
        var result = suppliers.Select(s => _mapper.ReverseMap(s));
        _logger.LogInformation("Retrieved {Count} suppliers", suppliers.Count());
        return result;
    }

    public async Task<JSONSupplier> GetSupplierByNameAsync(string name)
    {
        _logger.LogInformation("Retrieving supplier with name {Name}", name);
        var supplier = await _repository.GetByNameAsync(name);
        var result = _mapper.ReverseMap(supplier);
        _logger.LogInformation("Retrieved supplier with name {Name}", name);
        return result;
    }

    public async Task<JSONSupplier> CreateSupplierAsync(JSONSupplier jsonSupplier)
    {
        _logger.LogInformation("Validating and creating supplier with name {Name}", jsonSupplier.Name);
        await ValidateSupplierAsync(jsonSupplier);
        var supplier = _mapper.Map(jsonSupplier);
        var createdSupplier = await _repository.CreateAsync(supplier);
        _logger.LogInformation("Created supplier with ID {Id}", createdSupplier.Id);
        return _mapper.ReverseMap(createdSupplier);
    }

    public async Task UpdateSupplierAsync(string name, JSONSupplier jsonSupplier)
    {
        _logger.LogInformation("Updating supplier with name {Name}", name);
        var existingSupplier = await _repository.GetByNameAsync(name);

        await ValidateSupplierAsync(jsonSupplier, currentSupplierName: name);
        var updatedSupplier = _mapper.Map(jsonSupplier);
        updatedSupplier.Id = existingSupplier.Id; // Ensure the ID remains unchanged
        await _repository.UpdateByNameAsync(name, updatedSupplier);
        _logger.LogInformation("Updated supplier with name {Name}", name);
    }

    public async Task DeleteSupplierAsync(string id)
    {
        _logger.LogInformation("Deleting supplier with ID {Id}", id);
        await _repository.DeleteAsync(id);
        _logger.LogInformation("Deleted supplier with ID {Id}", id);
    }

    private async Task ValidateSupplierAsync(JSONSupplier supplier, string? currentSupplierName = null)
    {
        if (string.IsNullOrWhiteSpace(supplier.Name))
        {
            throw new BadRequestException("Supplier name is required and cannot be empty.");
        }

        // Check if supplier with same name already exists, excluding current supplier when updating.
        var allSuppliers = await _repository.GetAllAsync();
        if (allSuppliers.Any(s => s.Name.Equals(supplier.Name, StringComparison.OrdinalIgnoreCase)
                                   && !string.Equals(s.Name, currentSupplierName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidItemException($"Supplier with name '{supplier.Name}' already exists.");
        }
    }
}