using ProductService.Entities;

namespace ProductService.Services;

public interface IProductCacheService
{
    Task SetProductsAsync(string key, List<Product> products);

    Task<List<Product>?> GetProductsAsync(string key);

    Task RemoveAsync(string key);
}