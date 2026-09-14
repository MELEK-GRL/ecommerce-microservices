using ProductService.Entities;

namespace ProductService.Repositories;

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
}