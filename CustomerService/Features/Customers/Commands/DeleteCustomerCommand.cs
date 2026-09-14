using CustomerService.DTOs;
using MediatR;

namespace CustomerService.Features.Customers.Commands;

public class DeleteCustomerCommand : IRequest<GetCustomerDto?>
{
    public int Id { get; set; }
}