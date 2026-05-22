using FluentValidation;

namespace Mangefy.Application.Settings.BusinessSchedule.Commands.UpdateBusinessSchedule;

public sealed class UpdateBusinessScheduleCommandValidator : AbstractValidator<UpdateBusinessScheduleCommand>
{
    public UpdateBusinessScheduleCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();

        When(x => x.WeeklySchedule is not null, () =>
        {
            RuleForEach(x => x.WeeklySchedule)
                .ChildRules(day =>
                {
                    day.RuleFor(d => d.DayOfWeek)
                        .Must(d => Enum.TryParse<DayOfWeek>(d, out _))
                        .WithMessage("DayOfWeek inválido.");
                    day.When(d => !d.IsClosed, () =>
                    {
                        day.RuleFor(d => d.OpenTime).NotEmpty().WithMessage("Horário de abertura obrigatório.");
                        day.RuleFor(d => d.CloseTime).NotEmpty().WithMessage("Horário de fechamento obrigatório.");
                    });
                });
        });

        When(x => x.ClosingPolicy is not null, () =>
        {
            RuleFor(x => x.ClosingPolicy!.GracePeriodMinutes)
                .GreaterThanOrEqualTo(0).LessThanOrEqualTo(120);
        });
    }
}
