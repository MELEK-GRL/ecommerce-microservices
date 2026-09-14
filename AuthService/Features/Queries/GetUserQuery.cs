using AuthService.DTOs;
using MediatR;

namespace AuthService.Features.Queries;

public class GetUsersQuery : IRequest<List<GetUserDto>>
{
}