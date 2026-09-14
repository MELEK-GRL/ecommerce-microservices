using CustomerService.DTOs;
using MediatR;

namespace CustomerService.Features.Customers.Queries;

public class GetCustomersQuery : IRequest<List<GetCustomerDto>>
{
}