using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stock_Server.Data;
using Stock_Server.Models;
using Stock_Server.Util.Exceptions;

namespace Stock_Server.Repository;

public class ProductRepository : IRepository<Product>
{
    private readonly StockDbContext _context;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(StockDbContext context, ILogger<ProductRepository> logger)
    {
        _context = context ?? throw new RepositoryInitializationException("DbContext is null", new ArgumentNullException(nameof(context)));
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all products");
            var entities = await _context.Products.Include(p => p.Supplier).ToListAsync();
            _logger.LogInformation("Retrieved {Count} products", entities.Count);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all products");
            throw new DBException("Failed to retrieve all products", ex);
        }
    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        try
        {
            _logger.LogInformation("Retrieving product with ID {Id}", id);
            var entity = await _context.Products.Include(p => p.Supplier).FirstOrDefaultAsync(p => p.Id == id);
            if (entity == null)
            {
                _logger.LogWarning("Product with ID {Id} not found", id);
                return null;
            }
            _logger.LogInformation("Retrieved product with ID {Id}", id);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve product with ID {Id}", id);
            throw new DBException($"Failed to retrieve product with ID {id}", ex);
        }
    }

    public async Task<Product?> GetByNameAsync(string name)
    {
        try
        {
            _logger.LogInformation("Retrieving product with name {Name}", name);
            var entity = await _context.Products.Include(p => p.Supplier).FirstOrDefaultAsync(p => p.Name == name);
            if (entity == null)
            {
                _logger.LogWarning("Product with name {Name} not found", name);
                return null;
            }
            _logger.LogInformation("Retrieved product with name {Name}", name);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve product with name {Name}", name);
            throw new DBException($"Failed to retrieve product with name {name}", ex);
        }
    }

    public async Task<Product> CreateAsync(Product entity)
    {
        try
        {
            _logger.LogInformation("Creating new product with name {Name}", entity.Name);
            _context.Products.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created product with ID {Id}", entity.Id);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product");
            throw new DBException("Failed to create product", ex);
        }
    }



    public async Task UpdateByNameAsync(string name, Product entity)
    {
        try
        {
            _logger.LogInformation("Updating product with name {Name}", name);
            var existingEntity = await GetByNameAsync(name);
            if (existingEntity == null)
                throw new ItemNotFoundException($"Product with name {name} not found");
            entity.Id = existingEntity.Id;
            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated product with name {Name}", name);
        }
        catch (ItemNotFoundException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update product with name {Name}", name);
            throw new DBException($"Failed to update product with name {name}", ex);
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            _logger.LogInformation("Deleting product with ID {Id}", id);
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new ItemNotFoundException($"Product with ID {id} not found");
            _context.Products.Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted product with ID {Id}", id);
        }
        catch (ItemNotFoundException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete product with ID {Id}", id);
            throw new DBException($"Failed to delete product with ID {id}", ex);
        }
    }
        public async Task<bool> ExistsByNameAsync(string name, string? excludeId = null)
    {
        var query = _context.Products.Where(p => p.Name == name);
        if (!string.IsNullOrEmpty(excludeId))
            query = query.Where(p => p.Id != excludeId);
        return await query.AnyAsync();
    }

   
}