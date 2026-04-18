
namespace Stock_Server.Repository;

public interface IRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(string id);
    Task<T> GetByNameAsync(string name);
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task UpdateByNameAsync(string name, T entity);
    Task DeleteAsync(string id);
}