using FluentValidation;

namespace Mangefy.Application.Settings.ReservationSettings.Commands.UpdateReservationSettings;

public sealed class UpdateReservationSettingsCommandValidator : AbstractValidator<UpdateReservationSettingsCommand>
{
    public UpdateReservationSettingsCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        When(x => x.MaxSimultaneousReservations.HasValue, () =>
            RuleFor(x => x.MaxSimultaneousReservations!.Value).GreaterThan(0));
    }
}
