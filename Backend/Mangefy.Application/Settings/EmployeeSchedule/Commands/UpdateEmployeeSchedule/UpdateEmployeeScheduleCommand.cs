using MediatR;

namespace Mangefy.Application.Settings.EmployeeSchedule.Commands.UpdateEmployeeSchedule;

public sealed record DayShiftInput(string DayOfWeek, bool IsDayOff, string? StartTime, string? EndTime);

public sealed record UpdateEmployeeScheduleCommand(
    Guid TenantId,
    Guid EmployeeId,
    IReadOnlyList<DayShiftInput> WeeklyShifts) : IRequest;
