using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Common;
using Mangefy.Domain.EmployeeSchedules.Repositories;
using Mangefy.Domain.Employees.Repositories;
using Mangefy.Domain.Settings.Repositories;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Employees.Commands.GrantTemporaryAccess;

public sealed class GrantTemporaryAccessCommandHandler : IRequestHandler<GrantTemporaryAccessCommand>
{
    private readonly IEmployeeRepository _employees;
    private readonly IEmployeeScheduleRepository _schedules;
    private readonly ITenantRepository _tenants;
    private readonly IUnitOfWork _uow;

    public GrantTemporaryAccessCommandHandler(
        IEmployeeRepository employees,
        IEmployeeScheduleRepository schedules,
        ITenantRepository tenants,
        IUnitOfWork uow)
    {
        _employees = employees;
        _schedules = schedules;
        _tenants = tenants;
        _uow = uow;
    }

    public async Task Handle(GrantTemporaryAccessCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employees.GetByIdAsync(request.EmployeeId, cancellationToken)
            ?? throw new NotFoundException("Funcionário", request.EmployeeId);

        if (employee.TenantId != request.TenantId)
            throw new ForbiddenException("Acesso negado.");

        var schedule = await _schedules.GetByEmployeeIdAsync(request.TenantId, request.EmployeeId, cancellationToken)
            ?? throw new NotFoundException("Escala", request.EmployeeId);

        var today = DateTime.UtcNow.DayOfWeek;
        var shift = schedule.GetShift(today)
            ?? throw new DomainException("Funcionário não tem turno configurado para hoje.");

        if (!shift.IsWorkDay || shift.EndTime is null)
            throw new DomainException("Hoje é folga do funcionário — não é possível estender o turno.");

        var tenant = await _tenants.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException("Tenant", request.TenantId);

        // Acesso temporário = fim do turno no fuso do tenant + minutos de extensão solicitados pelo Owner
        var tz = TimeZoneInfo.FindSystemTimeZoneById(tenant.Timezone);
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var shiftEndLocal = nowLocal.Date.Add(shift.EndTime.Value.ToTimeSpan());
        var accessUntil = TimeZoneInfo.ConvertTimeToUtc(shiftEndLocal, tz).AddMinutes(request.ExtensionMinutes);

        employee.GrantTemporaryAccess(accessUntil);
        await _employees.UpdateAsync(employee, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
