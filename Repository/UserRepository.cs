using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stock_Server.Data;
using Stock_Server.Models;
using Stock_Server.Util.Exceptions;

namespace Stock_Server.Repository;

public class UserRepository : IRepository<User>
{
    private readonly StockDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(StockDbContext context, ILogger<UserRepository> logger)
    {
        _context = context ?? throw new RepositoryInitializationException("DbContext is null", new ArgumentNullException(nameof(context)));
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all users");
            var entities = await _context.Users.ToListAsync();
            _logger.LogInformation("Retrieved {Count} users", entities.Count);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all users");
            throw new DBException("Failed to retrieve all users", ex);
        }
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        try
        {
            _logger.LogInformation("Retrieving user with ID {Id}", id);
            var entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (entity == null)
            {
                _logger.LogWarning("User with ID {Id} not found", id);
                return null;
            }
            _logger.LogInformation("Retrieved user with ID {Id}", id);
            return entity;
        }
        catch (ItemNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user with ID {Id}", id);
            throw new DBException($"Failed to retrieve user with ID {id}", ex);
        }
    }

   

    public async Task<User> GetByRefreshTokenAsync(string refreshToken)
    {
        try
        {
            _logger.LogInformation("Retrieving user by refresh token");
            var entity = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (entity == null)
            {
                _logger.LogWarning("User with refresh token not found");
                throw new ItemNotFoundException("User with given refresh token not found");
            }
            _logger.LogInformation("Retrieved user by refresh token");
            return entity;
        }
        catch (ItemNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user by refresh token");
            throw new DBException("Failed to retrieve user by refresh token", ex);
        }
    }

    public async Task<User> CreateAsync(User entity)
    {
        try
        {
            _logger.LogInformation("Creating new user with username {Username}", entity.Username);
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created user with ID {Id}", entity.Id);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user");
            throw new DBException("Failed to create user", ex);
        }
    }

 

    public async Task DeleteAsync(string id)
    {
        try
        {
            _logger.LogInformation("Deleting user with ID {Id}", id);
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("User with ID {Id} not found for deletion", id);
                throw new ItemNotFoundException($"User with ID {id} not found");
            }
            _context.Users.Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted user with ID {Id}", id);
        }
        catch (ItemNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user with ID {Id}", id);
            throw new DBException($"Failed to delete user with ID {id}", ex);
        }
    }

public async Task<User?> GetByNameAsync(string name)
{
    try
    {
        _logger.LogInformation("Retrieving user with username {Username}", name);
        var entity = await _context.Users.FirstOrDefaultAsync(u => u.Username == name);
        if (entity == null)
        {
            _logger.LogWarning("User with username {Username} not found", name);
            return null;
        }
        _logger.LogInformation("Retrieved user with username {Username}", name);
        return entity;
    }
    catch (ItemNotFoundException)
    {
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to retrieve user with username {Username}", name);
        throw new DBException($"Failed to retrieve user with username {name}", ex);
    }
}

public async Task UpdateByNameAsync(string name, User entity)
{
    try
    {
        _logger.LogInformation("Updating user with username {Username}", name);
        var existingEntity = await _context.Users.FirstOrDefaultAsync(u => u.Username == name);
        if (existingEntity == null)
        {
            _logger.LogWarning("User with username {Username} not found for update", name);
            throw new ItemNotFoundException($"User with username {name} not found");
        }
        entity.Id = existingEntity.Id; // Preserve the ID
        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated user with username {Username}", name);
    }
    catch (ItemNotFoundException)
    {
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to update user with username {Username}", name);
        throw new DBException($"Failed to update user with username {name}", ex);
    }
}
    public async Task<bool> ExistsByNameAsync(string name, string? excludeId = null)
{
    var query = _context.Users.Where(u => u.Username == name);
    if (!string.IsNullOrEmpty(excludeId))
        query = query.Where(u => u.Id != excludeId);
    return await query.AnyAsync();
}
}

    
