using AuthService.Entities;

namespace AuthService.Repositories;

public interface IUserRepository
{
    Task<User> CreateAsync(
        User user,
        CancellationToken cancellationToken);

    Task<int> CreateUserWithOutboxAsync(
        User user,
        CancellationToken cancellationToken);

    Task<List<User>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken);
}