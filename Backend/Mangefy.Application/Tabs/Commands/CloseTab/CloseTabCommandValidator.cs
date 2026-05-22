using FluentValidation;

namespace Mangefy.Application.Tabs.Commands.CloseTab;

public sealed class CloseTabCommandValidator : AbstractValidator<CloseTabCommand>
{
    public CloseTabCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TabId).NotEmpty();
        RuleFor(x => x.Payments).NotEmpty().WithMessage("Informe ao menos um pagamento.");
        RuleForEach(x => x.Payments).ChildRules(p =>
        {
            p.RuleFor(x => x.Amount).GreaterThan(0);
        });
    }
}
