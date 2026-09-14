using CustomerService.DTOs;
using CustomerService.Repositories;
using MediatR;

namespace CustomerService.Features.Customers.Queries;

public class GetCustomersHandler : IRequestHandler<GetCustomersQuery, List<GetCustomerDto>>
{
    private readonly ICustomerRepository _repository;

    public GetCustomersHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<GetCustomerDto>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var customers = await _repository.GetAllAsync(cancellationToken);

        return customers.Select(x => new GetCustomerDto
        {
            Id = x.Id,
            UserId = x.UserId,
            FirstName = x.FirstName,
            LastName = x.LastName,
            Email = x.Email,
            Phone = x.Phone
        }).ToList();
    }
}