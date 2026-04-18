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

    public async Task<Supplier> GetByIdAsync(string id)
    {
        try
        {
            _logger.LogInformation("Retrieving supplier with ID {Id}", id);
            var entity = await _context.Suppliers.Include(s => s.Products).FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
            {
                _logger.LogWarning("Supplier with ID {Id} not found", id);
                throw new ItemNotFoundException($"Supplier with ID {id} not found");
            }
            _logger.LogInformation("Retrieved supplier with ID {Id}", id);
            return entity;
        }
        catch (ItemNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve supplier with ID {Id}", id);
            throw new DBException($"Failed to retrieve supplier with ID {id}", ex);
        }
    }

    public async Task<string> GetSupplierNameByIdAsync(string id)
    {
        try
        {
            _logger.LogInformation("Retrieving supplier name with ID {Id}", id);
            var name = await _context.Suppliers
                .Where(s => s.Id == id)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();
            if (name == null)
            {
                _logger.LogWarning("Supplier with ID {Id} not found", id);
                throw new ItemNotFoundException($"Supplier with ID {id} not found");
            }
            _logger.LogInformation("Retrieved supplier name with ID {Id}", id);
            return name;
        }
        catch (ItemNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve supplier name with ID {Id}", id);
            throw new DBException($"Failed to retrieve supplier name with ID {id}", ex);
        }
    }

    public async Task<Supplier> CreateAsync(Supplier entity)
    {
        try
        {
            _logger.LogInformation("Creating new supplier with name {Name}", entity.Name);
            // Note: Created new supplier entity
            _context.Suppliers.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created supplier with ID {Id}", entity.Id);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create supplier");
            throw new DBException("Failed to create supplier", ex);
        }
    }

    public async Task UpdateAsync(Supplier entity)
    {
        try
        {
            _logger.LogInformation("Updating supplier with ID {Id}", entity.Id);
            _context.Suppliers.Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated supplier with ID {Id}", entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update supplier with ID {Id}", entity.Id);
            throw new DBException($"Failed to update supplier with ID {entity.Id}", ex);
        }
    }

    public async Task<Supplier> GetByNameAsync(string name)
    {
        try
        {
            _logger.LogInformation("Retrieving supplier with name {Name}", name);
            var entity = await _context.Suppliers.Include(s => s.Products).FirstOrDefaultAsync(s => s.Name == name);
            if (entity == null)
            {
                _logger.LogWarning("Supplier with name {Name} not found", name);
                throw new ItemNotFoundException($"Supplier with name {name} not found");
            }
            _logger.LogInformation("Retrieved supplier with name {Name}", name);
            return entity;
        }
        catch (ItemNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve supplier with name {Name}", name);
            throw new DBException($"Failed to retrieve supplier with name {name}", ex);
        }
    }

    public async Task UpdateByNameAsync(string name, Supplier entity)
    {
        try
        {
            _logger.LogInformation("Updating supplier with name {Name}", name);
            var existingEntity = await GetByNameAsync(name);
            entity.Id = existingEntity.Id; // Preserve the ID
            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated supplier with name {Name}", name);
        }
        catch (ItemNotFoundException)
        {
            throw;
        }
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
            {
                _logger.LogWarning("Supplier with ID {Id} not found for deletion", id);
                throw new ItemNotFoundException($"Supplier with ID {id} not found");
            }
            _context.Suppliers.Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted supplier with ID {Id}", id);
        }
        catch (ItemNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete supplier with ID {Id}", id);
            throw new DBException($"Failed to delete supplier with ID {id}", ex);
        }
    }
}