using MediatR;
using ProductService.DTOs;
using ProductService.Repositories;

namespace ProductService.Features.Products.Commands;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, GetProductDto?>
{
    private readonly IProductRepository _repository;

    public DeleteProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetProductDto?> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _repository.DeleteAsync(
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