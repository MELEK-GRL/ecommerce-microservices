using MediatR;
using OrderService.DTOs;

namespace OrderService.Features.Orders.Queries;

public class GetOrdersQuery : IRequest<List<GetOrderDto>>
{
}