using Mangefy.Domain.EmployeeSchedules.Repositories;
using Mangefy.Domain.Employees.Repositories;
using MediatR;

namespace Mangefy.Application.Employees.Queries.GetActiveEmployees;

public sealed class GetActiveEmployeesQueryHandler
    : IRequestHandler<GetActiveEmployeesQuery, IReadOnlyList<ActiveEmployeeDto>>
{
    private readonly IEmployeeRepository _employees;
    private readonly IEmployeeScheduleRepository _schedules;

    public GetActiveEmployeesQueryHandler(
        IEmployeeRepository employees,
        IEmployeeScheduleRepository schedules)
    {
        _employees = employees;
        _schedules = schedules;
    }

    public async Task<IReadOnlyList<ActiveEmployeeDto>> Handle(
        GetActiveEmployeesQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var today = now.DayOfWeek;
        var currentTime = TimeOnly.FromDateTime(now);

        var allEmployees = await _employees.GetByTenantAsync(request.TenantId, cancellationToken);
        var allSchedules = await _schedules.GetByTenantIdAsync(request.TenantId, cancellationToken);

        var scheduleByEmployee = allSchedules.ToDictionary(s => s.EmployeeId);

        var result = new List<ActiveEmployeeDto>();

        foreach (var employee in allEmployees.Where(e => e.Status == Domain.Employees.EmployeeStatus.Active))
        {
            if (!scheduleByEmployee.TryGetValue(employee.Id, out var schedule))
                continue;

            var shift = schedule.GetShift(today);
            if (shift is null || !shift.IsWorkDay || shift.StartTime is null || shift.EndTime is null)
                continue;

            // Funcionário está dentro do turno OU tem acesso temporário ainda válido
            var withinShift = currentTime >= shift.StartTime.Value && currentTime <= shift.EndTime.Value;
            var hasTemporaryAccess = employee.HasTemporaryAccess();

            if (!withinShift && !hasTemporaryAccess)
                continue;

            result.Add(new ActiveEmployeeDto(
                employee.Id,
                employee.Name,
                shift.StartTime.Value,
                shift.EndTime.Value,
                employee.TemporaryAccessUntil));
        }

        return result;
    }
}
