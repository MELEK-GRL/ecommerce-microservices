using AuthService.DTOs;
using MediatR;

namespace AuthService.Features.Commands;

public class LoginCommand : IRequest<LoginResponseDto?>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}