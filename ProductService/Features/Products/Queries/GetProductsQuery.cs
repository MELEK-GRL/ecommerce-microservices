using MediatR;
using ProductService.DTOs;

namespace ProductService.Features.Products.Queries;

public class GetProductsQuery : IRequest<List<GetProductDto>>
{
}