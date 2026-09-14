using CustomerService.DTOs;
using CustomerService.Repositories;
using MediatR;

namespace CustomerService.Features.Customers.Commands;

public class DeleteCustomerHandler : IRequestHandler<DeleteCustomerCommand, GetCustomerDto?>
{
    private readonly ICustomerRepository _repository;

    public DeleteCustomerHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetCustomerDto?> Handle(
        DeleteCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await _repository.DeleteAsync(
            request.Id,
            cancellationToken);

        if (customer == null)
            return null;

        return new GetCustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone
        };
    }
}