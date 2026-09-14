using MediatR;
using ProductService.Entities;
using ProductService.Repositories;

namespace ProductService.Features.Products.Commands;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _repository;

    public CreateProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            CreatedAt = DateTime.UtcNow
        };

        var createdProduct = await _repository.CreateAsync(
            product,
            cancellationToken);

        return createdProduct.Id;
    }
}