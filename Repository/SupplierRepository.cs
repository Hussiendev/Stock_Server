using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stock_Server.Data;
using Stock_Server.Models;
using Stock_Server.Util.Exceptions;

namespace Stock_Server.Repository;

public class SupplierRepository : IRepository<Supplier>
{
    private readonly StockDbContext _context;
    private readonly ILogger<SupplierRepository> _logger;

    public SupplierRepository(StockDbContext context, ILogger<SupplierRepository> logger)
    {
        _context = context ?? throw new RepositoryInitializationException("DbContext is null", new ArgumentNullException(nameof(context)));
        _logger = logger;
    }

    public async Task<IEnumerable<Supplier>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all suppliers");
            var entities = await _context.Suppliers.Include(s => s.Products).ToListAsync();
            _logger.LogInformation("Retrieved {Count} suppliers", entities.Count);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all suppliers");
            throw new DBException("Failed to retrieve all suppliers", ex);
        }
    }

    public async Task<Supplier?> GetByIdAsync(string id)
    {
        try
        {
            _logger.LogInformation("Retrieving supplier with ID {Id}", id);
            var entity = await _context.Suppliers.Include(s => s.Products).FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
            {
                _logger.LogWarning("Supplier with ID {Id} not found", id);
                return null;
            }
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve supplier with ID {Id}", id);
            throw new DBException($"Failed to retrieve supplier with ID {id}", ex);
        }
    }

    public async Task<Supplier?> GetByNameAsync(string name)
    {
        try
        {
            _logger.LogInformation("Retrieving supplier with name {Name}", name);
            var entity = await _context.Suppliers.Include(s => s.Products).FirstOrDefaultAsync(s => s.Name == name);
            if (entity == null)
            {
                _logger.LogWarning("Supplier with name {Name} not found", name);
                return null;
            }
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve supplier with name {Name}", name);
            throw new DBException($"Failed to retrieve supplier with name {name}", ex);
        }
    }

    public async Task<Supplier> CreateAsync(Supplier entity)
    {
        try
        {
            _logger.LogInformation("Creating new supplier with name {Name}", entity.Name);
            _context.Suppliers.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create supplier");
            throw new DBException("Failed to create supplier", ex);
        }
    }

 

    public async Task UpdateByNameAsync(string name, Supplier entity)
    {
        try
        {
            _logger.LogInformation("Updating supplier with name {Name}", name);
            var existing = await GetByNameAsync(name);
            if (existing == null)
                throw new ItemNotFoundException($"Supplier with name {name} not found");
            entity.Id = existing.Id;
            _context.Entry(existing).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
        }
        catch (ItemNotFoundException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update supplier with name {Name}", name);
            throw new DBException($"Failed to update supplier with name {name}", ex);
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            _logger.LogInformation("Deleting supplier with ID {Id}", id);
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new ItemNotFoundException($"Supplier with ID {id} not found");
            _context.Suppliers.Remove(entity);
            await _context.SaveChangesAsync();
        }
        catch (ItemNotFoundException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete supplier with ID {Id}", id);
            throw new DBException($"Failed to delete supplier with ID {id}", ex);
        }
    }
    
    public async Task<bool> ExistsByNameAsync(string name, string? excludeId = null)
    {
        var query = _context.Suppliers.Where(s => s.Name == name);
        if (!string.IsNullOrEmpty(excludeId))
            query = query.Where(s => s.Id != excludeId);
        return await query.AnyAsync();
    }
}