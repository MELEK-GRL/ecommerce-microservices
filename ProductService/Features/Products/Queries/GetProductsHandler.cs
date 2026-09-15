using MediatR;
using ProductService.DTOs;
using ProductService.Repositories;
using ProductService.Services;

namespace ProductService.Features.Products.Queries;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, List<GetProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IProductCacheService _cache;

    public GetProductsHandler(
        IProductRepository repository,
        IProductCacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<GetProductDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var cachedProducts = await _cache.GetProductsAsync("products");

        if (cachedProducts != null)
        {
            return cachedProducts.Select(product => new GetProductDto 
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CreatedAt = product.CreatedAt
            }).ToList();
        }

        var products = await _repository.GetAllAsync(cancellationToken);

        await _cache.SetProductsAsync("products", products);

        return products.Select(product => new GetProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt
        }).ToList();
    }
}