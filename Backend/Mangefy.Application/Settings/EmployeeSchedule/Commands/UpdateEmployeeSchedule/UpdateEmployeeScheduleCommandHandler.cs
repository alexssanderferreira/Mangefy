using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.EmployeeSchedules.Repositories;
using Mangefy.Domain.Employees.Repositories;
using MediatR;
using DomainEmployeeSchedule = Mangefy.Domain.EmployeeSchedules.EmployeeSchedule;

namespace Mangefy.Application.Settings.EmployeeSchedule.Commands.UpdateEmployeeSchedule;

public sealed class UpdateEmployeeScheduleCommandHandler : IRequestHandler<UpdateEmployeeScheduleCommand>
{
    private readonly IEmployeeScheduleRepository _schedules;
    private readonly IEmployeeRepository _employees;
    private readonly IUnitOfWork _uow;

    public UpdateEmployeeScheduleCommandHandler(
        IEmployeeScheduleRepository schedules, IEmployeeRepository employees, IUnitOfWork uow)
    {
        _schedules = schedules;
        _employees = employees;
        _uow = uow;
    }

    public async Task Handle(UpdateEmployeeScheduleCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employees.GetByIdAsync(request.EmployeeId, cancellationToken)
            ?? throw new NotFoundException("Funcionário", request.EmployeeId);

        if (employee.TenantId != request.TenantId)
            throw new ForbiddenException();

        var schedule = await _schedules.GetByEmployeeIdAsync(request.TenantId, request.EmployeeId, cancellationToken);

        if (schedule is null)
        {
            schedule = DomainEmployeeSchedule.Create(request.TenantId, request.EmployeeId);
            await _schedules.AddAsync(schedule, cancellationToken);
        }

        foreach (var shift in request.WeeklyShifts)
        {
            var day = Enum.Parse<DayOfWeek>(shift.DayOfWeek);
            if (shift.IsDayOff)
                schedule.SetDayOff(day);
            else
                schedule.SetWorkDay(day, TimeOnly.Parse(shift.StartTime!), TimeOnly.Parse(shift.EndTime!));
        }

        await _schedules.UpdateAsync(schedule, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
