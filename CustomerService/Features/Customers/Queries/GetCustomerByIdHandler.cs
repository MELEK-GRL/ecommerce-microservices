using CustomerService.DTOs;
using CustomerService.Repositories;
using MediatR;

namespace CustomerService.Features.Customers.Queries;

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, GetCustomerDto?>
{
    private readonly ICustomerRepository _repository;

    public GetCustomerByIdHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetCustomerDto?> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (customer == null)
            return null;

        return new GetCustomerDto
        {
            Id = customer.Id,
            UserId = customer.UserId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone
        };
    }
}