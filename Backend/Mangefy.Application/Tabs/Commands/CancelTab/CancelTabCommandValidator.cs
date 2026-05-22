using FluentValidation;

namespace Mangefy.Application.Tabs.Commands.CancelTab;

public sealed class CancelTabCommandValidator : AbstractValidator<CancelTabCommand>
{
    public CancelTabCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TabId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
