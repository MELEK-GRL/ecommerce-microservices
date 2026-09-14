using System.Text.Json;
using AuthService.Data;
using AuthService.Entities;
using AuthService.Events;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<User> CreateAsync(
        User user,
        CancellationToken cancellationToken)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task<int> CreateUserWithOutboxAsync(
        User user,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. User'ı ekle
            await _context.Users.AddAsync(
                user,
                cancellationToken);

            // 2. User.Id oluşsun
            await _context.SaveChangesAsync(
                cancellationToken);

            // 3. UserRegisteredEvent oluştur
            var userRegisteredEvent = new UserRegisteredEvent
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Age = user.Age,
                Email = user.Email
            };

            // 4. Event'i JSON'a çevir
            var outboxMessage = new OutboxMessage
            {
                Type = "UserRegistered",
                Payload = JsonSerializer.Serialize(userRegisteredEvent),
                CreatedAt = DateTime.UtcNow
            };

            // 5. Outbox'a ekle
            await _context.OutboxMessages.AddAsync(
                outboxMessage,
                cancellationToken);

            // 6. Outbox'ı kaydet
            await _context.SaveChangesAsync(
                cancellationToken);

            // 7. İkisini birlikte kesinleştir
            await transaction.CommitAsync(
                cancellationToken);

            return user.Id;
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    public async Task<List<User>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Users
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(
                x => x.Email == email,
                cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return await _context.Users
            .AnyAsync(
                x => x.Email == email,
                cancellationToken);
    }
}