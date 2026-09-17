using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
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

            var dbContext = scope.ServiceProvider
                .GetRequiredService<ProductDbContext>();

            // Daha önce bu Order işlendi mi?
            var alreadyProcessed = await dbContext.ProcessedOrders
                .AnyAsync(
                    x => x.OrderId == orderCreatedEvent.OrderId,
                    CancellationToken.None);

            if (alreadyProcessed)
            {
                Console.WriteLine(
                    $"OrderId {orderCreatedEvent.OrderId} daha önce işlendi.");

                await channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);

                return;
            }

            // Stok düşür
            foreach (var item in orderCreatedEvent.Items)
            {
                await repository.DecreaseStockAsync(
                    item.ProductId,
                    item.Quantity,
                    CancellationToken.None);
            }

            // İşlenen Order'ı kaydet
            var processedOrder = new ProductService.Entities.ProcessedOrder
            {
                OrderId = orderCreatedEvent.OrderId,
                ProcessedAt = DateTime.UtcNow
            };

            await dbContext.ProcessedOrders.AddAsync(
                processedOrder,
                CancellationToken.None);

            await dbContext.SaveChangesAsync(CancellationToken.None);

            // Redis cache'i temizle
            await cache.RemoveAsync("products");

            Console.WriteLine(
                $"Stok güncellendi. OrderId: {orderCreatedEvent.OrderId}");

            // RabbitMQ ACK
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