using FluentValidation;

namespace Mangefy.Application.Tabs.Commands.ReturnOrderItem;

public sealed class ReturnOrderItemCommandValidator : AbstractValidator<ReturnOrderItemCommand>
{
    public ReturnOrderItemCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TabId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}
