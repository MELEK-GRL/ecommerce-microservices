using CustomerService.Data;
using CustomerService.Entities;
using Microsoft.EntityFrameworkCore;
namespace CustomerService.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly CustomerDbContext _context;

    public CustomerRepository(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task<Customer> CreateAsync(
        Customer customer,
        CancellationToken cancellationToken)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return customer;
    }
    public async Task<List<Customer>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Customers
            .ToListAsync(cancellationToken);
    }
    public async Task<Customer?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
    public async Task<Customer?> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (customer == null)
            return null;

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return customer;
    }
}