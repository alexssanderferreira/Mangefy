using FluentValidation;

namespace Mangefy.Application.Tabs.Commands.CancelOrderItem;

public sealed class CancelOrderItemCommandValidator : AbstractValidator<CancelOrderItemCommand>
{
    public CancelOrderItemCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TabId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}
