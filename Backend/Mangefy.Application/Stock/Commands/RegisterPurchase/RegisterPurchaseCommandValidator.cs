using FluentValidation;

namespace Mangefy.Application.Stock.Commands.RegisterPurchase;

public sealed class RegisterPurchaseCommandValidator : AbstractValidator<RegisterPurchaseCommand>
{
    public RegisterPurchaseCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.StockItemId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Reason).MaximumLength(500).When(x => x.Reason is not null);
    }
}
