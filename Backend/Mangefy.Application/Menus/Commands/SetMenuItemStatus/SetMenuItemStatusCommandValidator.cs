using FluentValidation;
using Mangefy.Domain.Menus;

namespace Mangefy.Application.Menus.Commands.SetMenuItemStatus;

public sealed class SetMenuItemStatusCommandValidator : AbstractValidator<SetMenuItemStatusCommand>
{
    public SetMenuItemStatusCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MenuId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Status).Must(s => Enum.TryParse<MenuItemStatus>(s, out _))
            .WithMessage("Status inválido. Use Available, Unavailable ou OutOfStock.");
    }
}
