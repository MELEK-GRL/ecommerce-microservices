using AuthService.DTOs;
using AuthService.Features.Commands;
using AuthService.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto dto)
    {
        try
        {
            var command = new RegisterUserCommand
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Age = dto.Age,
                Email = dto.Email,
                Password = dto.Password
            };

            var userId = await _mediator.Send(command);

            return Ok(userId);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var query = new GetUsersQuery();

        var users = await _mediator.Send(query);

        return Ok(users);
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var command = new LoginCommand
        {
            Email = dto.Email,
            Password = dto.Password
        };

        var result = await _mediator.Send(command);

        if (result == null)
            return Unauthorized("Email veya şifre hatalı.");

        return Ok(result);
    }
}