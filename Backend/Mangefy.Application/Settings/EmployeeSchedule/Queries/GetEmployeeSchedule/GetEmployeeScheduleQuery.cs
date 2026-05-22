using MediatR;

namespace Mangefy.Application.Settings.EmployeeSchedule.Queries.GetEmployeeSchedule;

public sealed record DayShiftDto(string DayOfWeek, bool IsWorkDay, string? StartTime, string? EndTime);

public sealed record EmployeeScheduleDto(Guid Id, Guid EmployeeId, IReadOnlyList<DayShiftDto> WeeklyShifts);

public sealed record GetEmployeeScheduleQuery(Guid TenantId, Guid EmployeeId)
    : IRequest<EmployeeScheduleDto?>;
