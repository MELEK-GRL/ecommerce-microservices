using CustomerService.DTOs;
using MediatR;

namespace CustomerService.Features.Customers.Queries;

public class GetCustomerByIdQuery : IRequest<GetCustomerDto?>
{
    public int Id { get; set; }
}