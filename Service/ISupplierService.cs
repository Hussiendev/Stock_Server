using Stock_Server.Mapper;

public interface ISupplierService
{
    Task<IEnumerable<JSONSupplier>> GetAllSuppliersAsync();
    Task<JSONSupplier> GetSupplierByNameAsync(string name);
    Task<JSONSupplier> CreateSupplierAsync(JSONSupplier jsonSupplier);
    Task UpdateSupplierAsync(string name, JSONSupplier jsonSupplier);
     Task DeleteSupplierAsync(string id);
}