using FluentValidation;
using Mangefy.Domain.Menus;

namespace Mangefy.Application.Menus.Commands.UpdateMenuItem;

public sealed class UpdateMenuItemCommandValidator : AbstractValidator<UpdateMenuItemCommand>
{
    public UpdateMenuItemCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MenuId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Station).Must(s => Enum.TryParse<MenuItemStation>(s, out _))
            .WithMessage("Estação inválida. Use Kitchen, Bar ou Custom.");
    }
}
