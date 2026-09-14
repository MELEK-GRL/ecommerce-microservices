using System.Text;
using OrderService.Data;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace OrderService.BackgroundServices;

public class OutboxPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OutboxPublisher(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        await using var connection =
            await factory.CreateConnectionAsync();

        await using var channel =
            await connection.CreateChannelAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<OrderDbContext>();

            var messages = await dbContext.OutboxMessages
                .Where(x => x.ProcessedAt == null)
                .ToListAsync(stoppingToken);

            foreach (var message in messages)
            {
                var body = Encoding.UTF8.GetBytes(
                    message.Payload);

                await channel.BasicPublishAsync(
                    exchange: "ecommerce.events",
                    routingKey: "order.created",
                    body: body);

                message.ProcessedAt = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(
                    stoppingToken);

                Console.WriteLine(
                    $"Order mesajı RabbitMQ'ya gönderildi: {message.Id}");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }
    }
}