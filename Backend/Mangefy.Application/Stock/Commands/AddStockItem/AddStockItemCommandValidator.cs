using FluentValidation;

namespace Mangefy.Application.Stock.Commands.AddStockItem;

public sealed class AddStockItemCommandValidator : AbstractValidator<AddStockItemCommand>
{
    public AddStockItemCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MinimumQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CostPerUnit).GreaterThanOrEqualTo(0);
    }
}
