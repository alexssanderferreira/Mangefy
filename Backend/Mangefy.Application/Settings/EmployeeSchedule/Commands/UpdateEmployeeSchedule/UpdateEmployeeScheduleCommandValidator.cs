using FluentValidation;

namespace Mangefy.Application.Settings.EmployeeSchedule.Commands.UpdateEmployeeSchedule;

public sealed class UpdateEmployeeScheduleCommandValidator : AbstractValidator<UpdateEmployeeScheduleCommand>
{
    public UpdateEmployeeScheduleCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.WeeklyShifts).NotEmpty();
        RuleForEach(x => x.WeeklyShifts).ChildRules(shift =>
        {
            shift.RuleFor(s => s.DayOfWeek)
                .Must(d => Enum.TryParse<DayOfWeek>(d, out _))
                .WithMessage("DayOfWeek inválido.");
            shift.When(s => !s.IsDayOff, () =>
            {
                shift.RuleFor(s => s.StartTime).NotEmpty().WithMessage("Horário de início obrigatório.");
                shift.RuleFor(s => s.EndTime).NotEmpty().WithMessage("Horário de término obrigatório.");
            });
        });
    }
}
