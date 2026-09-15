using ProductService.Entities;

public interface IProductRepository
{
    Task<Product> CreateAsync(
        Product product,
        CancellationToken cancellationToken);

    Task<List<Product>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<Product?> DeleteAsync(
        int id,
        CancellationToken cancellationToken);

    Task<Product?> DecreaseStockAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken);
}