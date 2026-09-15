using System.Text;
using System.Text.Json;
using ProductService.Events;
using ProductService.Repositories;
using ProductService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductService.BackgroundServices;

public class ProductServiceConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ProductServiceConsumer(IServiceScopeFactory scopeFactory)
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
            queue: "product-service-queue",
            durable: true,
            exclusive: false,
            autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            var body = args.Body.ToArray();

            var message = Encoding.UTF8.GetString(body);

            var orderCreatedEvent =
                JsonSerializer.Deserialize<OrderCreatedEvent>(message);

            if (orderCreatedEvent == null)
                return;

            using var scope = _scopeFactory.CreateScope();

            var repository = scope.ServiceProvider
                .GetRequiredService<IProductRepository>();
            
            var cache = scope.ServiceProvider
                .GetRequiredService<IProductCacheService>();

            foreach (var item in orderCreatedEvent.Items)
            {
                await repository.DecreaseStockAsync(
                    item.ProductId,
                    item.Quantity,
                    CancellationToken.None);
            }
            Console.WriteLine("Redis cache silindi: products");
            await cache.RemoveAsync("products");
            Console.WriteLine("Redis cache silindi: products");

            Console.WriteLine(
                $"Stok güncellendi. OrderId: {orderCreatedEvent.OrderId}");

            await channel.BasicAckAsync(
                deliveryTag: args.DeliveryTag,
                multiple: false);
        };

        await channel.BasicConsumeAsync(
            queue: "product-service-queue",
            autoAck: false,
            consumer: consumer);

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }
}