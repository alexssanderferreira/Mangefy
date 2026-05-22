using FluentValidation;

namespace Mangefy.Application.Menus.Commands.AddMenuCategory;

public sealed class AddMenuCategoryCommandValidator : AbstractValidator<AddMenuCategoryCommand>
{
    public AddMenuCategoryCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MenuId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
