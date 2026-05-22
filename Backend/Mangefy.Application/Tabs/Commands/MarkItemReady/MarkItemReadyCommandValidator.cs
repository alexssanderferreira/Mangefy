using FluentValidation;

namespace Mangefy.Application.Tabs.Commands.MarkItemReady;

public sealed class MarkItemReadyCommandValidator : AbstractValidator<MarkItemReadyCommand>
{
    public MarkItemReadyCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TabId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}
