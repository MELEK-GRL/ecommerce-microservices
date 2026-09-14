using CustomerService.Entities;

namespace CustomerService.Repositories;

public interface ICustomerRepository
{
    Task<Customer> CreateAsync(
        Customer customer,
        CancellationToken cancellationToken);

    Task<List<Customer>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<Customer?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<Customer?> DeleteAsync(
        int id,
        CancellationToken cancellationToken);
}