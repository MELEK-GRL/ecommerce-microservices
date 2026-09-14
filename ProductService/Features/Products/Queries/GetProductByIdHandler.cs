using MediatR;
using ProductService.DTOs;
using ProductService.Repositories;

namespace ProductService.Features.Products.Queries;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, GetProductDto?>
{
    private readonly IProductRepository _repository;

    public GetProductByIdHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetProductDto?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (product == null)
            return null;

        return new GetProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt
        };
    }
}