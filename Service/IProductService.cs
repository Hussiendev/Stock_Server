using Stock_Server.Mapper;


namespace Stock_Server.Service;

public interface IProductService
{
   Task<IEnumerable<JSONProduct>> GetAllProductsAsync();
   Task<JSONProduct> GetProductByNameAsync(string name);
   Task<JSONProduct> CreateProductAsync(JSONProduct jsonProduct);
    Task UpdateProductAsync(string name, UpdateJSONProduct updateRequest);
     Task DeleteProductAsync(string id);
      Task<decimal> GetCapital(string name);



}