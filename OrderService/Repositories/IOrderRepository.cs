using OrderService.Entities;

namespace OrderService.Repositories;

public interface IOrderRepository
{
    Task<Order> CreateAsync(
        Order order,
        CancellationToken cancellationToken);

    Task<List<Order>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);
    Task<Order?> DeleteAsync(
        int id,
        CancellationToken cancellationToken);
}