using MediatR;
using OrderService.DTOs;

namespace OrderService.Features.Orders.Commands;

public class DeleteOrderCommand : IRequest<GetOrderDto?>
{
    public int Id { get; set; }
}