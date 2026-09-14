using AuthService.DTOs;
using AuthService.Repositories;
using MediatR;

namespace AuthService.Features.Queries;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, List<GetUserDto>>
{
    private readonly IUserRepository _repository;

    public GetUsersHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<GetUserDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _repository.GetAllAsync(cancellationToken);

        return users.Select(user => new GetUserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Age = user.Age,
            Email = user.Email,
            Role = user.Role
        }).ToList();
    }
}