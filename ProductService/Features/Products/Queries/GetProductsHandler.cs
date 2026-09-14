using MediatR;
using ProductService.DTOs;
using ProductService.Repositories;

namespace ProductService.Features.Products.Queries;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, List<GetProductDto>>
{
    private readonly IProductRepository _repository;

    public GetProductsHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<GetProductDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync(cancellationToken);

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