using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Entities;

namespace ProductService.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<Product> CreateAsync(
        Product product,
        CancellationToken cancellationToken)
    {
        await _context.Products.AddAsync(product, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return product;
    }

    public async Task<List<Product>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<Product?> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (product == null)
            return null;

        _context.Products.Remove(product);

        await _context.SaveChangesAsync(cancellationToken);

        return product;
    }
    public async Task<Product?> DecreaseStockAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(
                x => x.Id == productId,
                cancellationToken);

        if (product == null)
            return null;

        product.Stock -= quantity;

        await _context.SaveChangesAsync(
            cancellationToken);

        return product;
    }
}