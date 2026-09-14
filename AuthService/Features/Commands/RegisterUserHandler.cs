using AuthService.Entities;
using AuthService.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Features.Commands;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, int>
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public RegisterUserHandler(
        IUserRepository repository,
        IPasswordHasher<User> passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<int> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var emailExists = await _repository.ExistsByEmailAsync(
            request.Email,
            cancellationToken);

        if (emailExists)
            throw new InvalidOperationException(
                "Bu email zaten kayıtlı.");

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Age = request.Age,
            Email = request.Email,
            Role = "User"
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        var userId = await _repository.CreateUserWithOutboxAsync(
            user,
            cancellationToken);

        return userId;
    }
}