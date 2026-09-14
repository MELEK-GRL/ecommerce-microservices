using MediatR;
using OrderService.DTOs;

namespace OrderService.Features.Orders.Queries;

public class GetOrderByIdQuery : IRequest<GetOrderDto?>
{
    public int Id { get; set; }
}