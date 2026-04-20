using Microsoft.Extensions.Logging;
using Stock_Server.Mapper;
using Stock_Server.Models;
using Stock_Server.Repository;
using Stock_Server.Util.Exceptions;

namespace Stock_Server.Service;

public class SupplierService : ISupplierService
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
        var suppliers = await _repository.GetAllAsync();
        return suppliers.Select(s => _mapper.ReverseMap(s));
    }

    public async Task<JSONSupplier> GetSupplierByNameAsync(string name)
    {
        var supplier = await _repository.GetByNameAsync(name);
        if (supplier == null)
            throw new ItemNotFoundException($"Supplier '{name}' not found");
        return _mapper.ReverseMap(supplier);
    }

    public async Task<JSONSupplier> CreateSupplierAsync(JSONSupplier jsonSupplier)
    {
        await ValidateSupplierAsync(jsonSupplier);
        var supplier = _mapper.Map(jsonSupplier);
        var created = await _repository.CreateAsync(supplier);
        return _mapper.ReverseMap(created);
    }

    public async Task UpdateSupplierAsync(string name, JSONSupplier jsonSupplier)
    {
        var existing = await _repository.GetByNameAsync(name);
        if (existing == null)
            throw new ItemNotFoundException($"Supplier '{name}' not found");
        await ValidateSupplierAsync(jsonSupplier, name);
        var updated = _mapper.Map(jsonSupplier);
        updated.Id = existing.Id;
        await _repository.UpdateByNameAsync(name, updated);
    }

    public async Task DeleteSupplierAsync(string id)
    {
        await _repository.DeleteAsync(id);
    }

    private async Task ValidateSupplierAsync(JSONSupplier supplier, string? currentName = null)
    {
        if (string.IsNullOrWhiteSpace(supplier.Name))
            throw new BadRequestException("Supplier name is required.");
        var exists = await _repository.ExistsByNameAsync(supplier.Name, currentName);
        if (exists)
            throw new InvalidItemException($"Supplier '{supplier.Name}' already exists.");
    }
}