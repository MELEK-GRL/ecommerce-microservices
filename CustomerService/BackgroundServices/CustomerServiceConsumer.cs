using System.Text;
using System.Text.Json;
using CustomerService.Entities;
using CustomerService.Events;
using CustomerService.Repositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CustomerService.BackgroundServices;

public class CustomerServiceConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CustomerServiceConsumer(IServiceScopeFactory scopeFactory)
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

        await channel.QueueDeclareAsync(
            queue: "customer-service-queue",
            durable: true,
            exclusive: false,
            autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            var body = args.Body.ToArray();

            var message = Encoding.UTF8.GetString(body);

            var userRegisteredEvent =
                JsonSerializer.Deserialize<UserRegisteredEvent>(message);

            if (userRegisteredEvent == null)
                return;

            using var scope = _scopeFactory.CreateScope();

            var repository = scope.ServiceProvider
                .GetRequiredService<ICustomerRepository>();

            var customer = new Customer
            {
                UserId = userRegisteredEvent.UserId,
                FirstName = userRegisteredEvent.FirstName,
                LastName = userRegisteredEvent.LastName,
                Email = userRegisteredEvent.Email,
                Phone = string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await repository.CreateAsync(
                customer,
                CancellationToken.None);

            Console.WriteLine(
                $"Customer oluşturuldu: {customer.FirstName} {customer.LastName}");

            await channel.BasicAckAsync(
                deliveryTag: args.DeliveryTag,
                multiple: false);
        };

        await channel.BasicConsumeAsync(
            queue: "customer-service-queue",
            autoAck: false,
            consumer: consumer);

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }
}