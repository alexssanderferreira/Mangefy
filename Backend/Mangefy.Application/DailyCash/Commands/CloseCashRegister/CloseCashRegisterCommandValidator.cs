using FluentValidation;

namespace Mangefy.Application.DailyCash.Commands.CloseCashRegister;

public sealed class CloseCashRegisterCommandValidator : AbstractValidator<CloseCashRegisterCommand>
{
    public CloseCashRegisterCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.MethodBalances).NotEmpty();
        RuleForEach(x => x.MethodBalances).ChildRules(m =>
        {
            m.RuleFor(b => b.ExpectedAmount).GreaterThanOrEqualTo(0);
            m.RuleFor(b => b.CountedAmount).GreaterThanOrEqualTo(0);
        });
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes is not null);
    }
}
