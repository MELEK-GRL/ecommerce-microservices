using System.Text.Json;
using ProductService.Entities;
using StackExchange.Redis;

namespace ProductService.Services;

public class ProductCacheService : IProductCacheService
{
    private readonly IConnectionMultiplexer _redis;

    public ProductCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task SetProductsAsync(
        string key,
        List<Product> products)
    {
        var database = _redis.GetDatabase();

        var json = JsonSerializer.Serialize(products);

        await database.StringSetAsync(key, json);
    }

    public async Task<List<Product>?> GetProductsAsync(string key)
    {
        var database = _redis.GetDatabase();

        var json = await database.StringGetAsync(key);

        if (json.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<List<Product>>(json!);
    }

    public async Task RemoveAsync(string key)
    {
        var database = _redis.GetDatabase();

        await database.KeyDeleteAsync(key);
    }
}