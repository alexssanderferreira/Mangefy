using FluentValidation;

namespace Mangefy.Application.Menus.Commands.CreateMenu;

public sealed class CreateMenuCommandValidator : AbstractValidator<CreateMenuCommand>
{
    public CreateMenuCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        When(x => x.ScheduleDays is not null || x.ScheduleStart is not null || x.ScheduleEnd is not null, () =>
        {
            RuleFor(x => x.ScheduleDays).NotEmpty().WithMessage("Dias da semana são obrigatórios quando horário é informado.");
            RuleFor(x => x.ScheduleStart).NotNull().WithMessage("Horário de início é obrigatório.");
            RuleFor(x => x.ScheduleEnd).NotNull().WithMessage("Horário de fim é obrigatório.");
        });
    }
}
