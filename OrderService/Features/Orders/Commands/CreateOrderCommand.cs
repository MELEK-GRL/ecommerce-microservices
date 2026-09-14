using MediatR;

namespace OrderService.Features.Orders.Commands;

public class CreateOrderCommand : IRequest<int>
{
    public int CustomerId { get; set; }

    public List<CreateOrderItemCommand> Items { get; set; } = new();
}

public class CreateOrderItemCommand
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}