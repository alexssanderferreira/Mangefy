using FluentValidation;

namespace Mangefy.Application.Tabs.Commands.StartItemPreparation;

public sealed class StartItemPreparationCommandValidator : AbstractValidator<StartItemPreparationCommand>
{
    public StartItemPreparationCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TabId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}
