using MediatR;
using ProductService.DTOs;

namespace ProductService.Features.Products.Commands;

public class DeleteProductCommand : IRequest<GetProductDto?>
{
    public int Id { get; set; }
}