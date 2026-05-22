using FluentValidation;

namespace Mangefy.Application.Settings.TabSettings.Commands.UpdateTabSettings;

public sealed class UpdateTabSettingsCommandValidator : AbstractValidator<UpdateTabSettingsCommand>
{
    public UpdateTabSettingsCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MinTabNumber).GreaterThan(0);
        RuleFor(x => x.MaxTabNumber).GreaterThan(x => x.MinTabNumber)
            .WithMessage("Número máximo deve ser maior que o mínimo.");
    }
}
