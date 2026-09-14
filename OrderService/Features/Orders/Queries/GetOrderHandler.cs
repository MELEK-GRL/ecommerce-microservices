using MediatR;
using OrderService.DTOs;
using OrderService.Repositories;

namespace OrderService.Features.Orders.Queries;

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, List<GetOrderDto>>
{
    private readonly IOrderRepository _repository;

    public GetOrdersHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<GetOrderDto>> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _repository.GetAllAsync(cancellationToken);

        return orders.Select(order => new GetOrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(item => new GetOrderItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        }).ToList();
    }
}