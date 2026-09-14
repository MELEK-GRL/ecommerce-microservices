using FluentValidation;

namespace OrderService.Features.Orders.Commands;

public class CreateOrderCommandValidator
    : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0);

        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.ProductId)
                    .GreaterThan(0);

                item.RuleFor(x => x.Quantity)
                    .GreaterThan(0);

                item.RuleFor(x => x.UnitPrice)
                    .GreaterThan(0);
            });
    }
}