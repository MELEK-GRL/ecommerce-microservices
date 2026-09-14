using MediatR;
using OrderService.DTOs;
using OrderService.Repositories;

namespace OrderService.Features.Orders.Queries;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, GetOrderDto?>
{
    private readonly IOrderRepository _repository;

    public GetOrderByIdHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetOrderDto?> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (order == null)
            return null;

        return new GetOrderDto
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
        };
    }
}