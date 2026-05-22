using FluentValidation;

namespace Mangefy.Application.DailyCash.Commands.RegisterWithdrawal;

public sealed class RegisterWithdrawalCommandValidator : AbstractValidator<RegisterWithdrawalCommand>
{
    public RegisterWithdrawalCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(300);
    }
}
