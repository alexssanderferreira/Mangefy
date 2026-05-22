using FluentValidation;

namespace Mangefy.Application.WorkforceSettings.Commands.UpdateWorkforceSettings;

public sealed class UpdateWorkforceSettingsCommandValidator : AbstractValidator<UpdateWorkforceSettingsCommand>
{
    public UpdateWorkforceSettingsCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ShiftToleranceMinutes).GreaterThanOrEqualTo(0).LessThanOrEqualTo(480);
    }
}
