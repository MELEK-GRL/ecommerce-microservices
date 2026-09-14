using CustomerService.Features.Customers.Commands;
using CustomerService.Features.Customers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var customers = await _mediator.Send(
            new GetCustomersQuery(),
            cancellationToken);

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var customer = await _mediator.Send(
            new GetCustomerByIdQuery { Id = id },
            cancellationToken);

        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteCustomerCommand { Id = id },
            cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(new
        {
            message = "İşte sildiğim müşteri:",
            customer = result
        });
    }
}