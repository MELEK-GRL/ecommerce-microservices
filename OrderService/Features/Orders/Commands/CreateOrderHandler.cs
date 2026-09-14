using MediatR;
using OrderService.Entities;
using OrderService.Repositories;

namespace OrderService.Features.Orders.Commands;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IOrderRepository _repository;

    public CreateOrderHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = new Order
        {
            CustomerId = request.CustomerId,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            Items = request.Items.Select(x => new OrderItem
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(x =>
            x.Quantity * x.UnitPrice);

        var createdOrder = await _repository.CreateWithOutboxAsync(
            order,
            cancellationToken);

        return createdOrder.Id;
    }
}