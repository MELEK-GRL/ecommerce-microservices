using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Entities;

namespace OrderService.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        await _context.Orders.AddAsync(order, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return order;
    }

    public async Task<List<Order>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Include(x => x.Items)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }
    public async Task<Order?> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (order == null)
            return null;

        _context.Orders.Remove(order);

        await _context.SaveChangesAsync(cancellationToken);

        return order;
    }
}