using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Features.Orders.Commands;
using OrderService.Features.Orders.Queries;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateOrderDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand
        {
            CustomerId = dto.CustomerId,
            Items = dto.Items.Select(x => new CreateOrderItemCommand
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice
            }).ToList()
        };

        var id = await _mediator.Send(command, cancellationToken);

        return Ok(new { id });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var orders = await _mediator.Send(
            new GetOrdersQuery(),
            cancellationToken);

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(
            new GetOrderByIdQuery { Id = id },
            cancellationToken);

        if (order == null)
            return NotFound();

        return Ok(order);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteOrderCommand { Id = id },
            cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(new
        {
            message = "İşte sildiğim sipariş:",
            order = result
        });
    }
}