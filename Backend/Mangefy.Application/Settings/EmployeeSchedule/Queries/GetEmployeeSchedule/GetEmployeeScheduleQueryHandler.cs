using Mangefy.Domain.EmployeeSchedules.Repositories;
using MediatR;

namespace Mangefy.Application.Settings.EmployeeSchedule.Queries.GetEmployeeSchedule;

public sealed class GetEmployeeScheduleQueryHandler
    : IRequestHandler<GetEmployeeScheduleQuery, EmployeeScheduleDto?>
{
    private readonly IEmployeeScheduleRepository _schedules;

    public GetEmployeeScheduleQueryHandler(IEmployeeScheduleRepository schedules)
        => _schedules = schedules;

    public async Task<EmployeeScheduleDto?> Handle(
        GetEmployeeScheduleQuery request, CancellationToken cancellationToken)
    {
        var schedule = await _schedules.GetByEmployeeIdAsync(
            request.TenantId, request.EmployeeId, cancellationToken);

        if (schedule is null) return null;

        var shifts = schedule.WeeklyShifts.Select(s => new DayShiftDto(
            s.DayOfWeek.ToString(),
            s.IsWorkDay,
            s.StartTime?.ToString("HH:mm"),
            s.EndTime?.ToString("HH:mm"))).ToList();

        return new EmployeeScheduleDto(schedule.Id, schedule.EmployeeId, shifts);
    }
}
