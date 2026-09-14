using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Features.Products.Commands;
using ProductService.Features.Products.Queries;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock
        };

        var id = await _mediator.Send(
            command,
            cancellationToken);

        return Ok(new { id });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var products = await _mediator.Send(
            new GetProductsQuery(),
            cancellationToken);

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _mediator.Send(
            new GetProductByIdQuery { Id = id },
            cancellationToken);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteProductCommand { Id = id },
            cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(new
        {
            message = "İşte sildiğim ürün:",
            product = result
        });
    }
}