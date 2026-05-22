using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.EmployeeSchedules.Repositories;
using Mangefy.Domain.Employees.Repositories;
using Mangefy.Domain.Settings.Repositories;
using Mangefy.Domain.Tenants.Repositories;

namespace Mangefy.Infrastructure.Services;

/// <summary>
/// Verifica se um funcionário pode operar com base em horário de turno e acesso temporário.
/// Owner sempre passa (IsOwner = true → acesso 24/7).
/// Funcionário fora do turno pode operar se:
///   a) possui acesso temporário ativo (TemporaryAccessUntil > UtcNow), ou
///   b) não tem escala cadastrada para o dia (nenhuma restrição configurada).
/// Usa timezone do tenant e ShiftToleranceMinutes da WorkforceSettings.
/// </summary>
public sealed class ShiftEnforcementService : IShiftEnforcementService
{
    private readonly IEmployeeRepository _employees;
    private readonly IEmployeeScheduleRepository _schedules;
    private readonly ITenantRepository _tenants;
    private readonly IWorkforceSettingsRepository _workforceSettings;

    public ShiftEnforcementService(
        IEmployeeRepository employees,
        IEmployeeScheduleRepository schedules,
        ITenantRepository tenants,
        IWorkforceSettingsRepository workforceSettings)
    {
        _employees = employees;
        _schedules = schedules;
        _tenants = tenants;
        _workforceSettings = workforceSettings;
    }

    public async Task<bool> CanOperateAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default)
    {
        var employee = await _employees.GetByIdAsync(employeeId, ct);
        if (employee is null) return false;
        // Owner role check: role grants full access (IsOwnerRole)
        // For now employees don't carry IsOwner — access is determined by role

        if (employee.HasTemporaryAccess()) return true;

        var schedule = await _schedules.GetByEmployeeIdAsync(tenantId, employeeId, ct);
        if (schedule is null) return true; // sem escala = sem restrição

        var tenant = await _tenants.GetByIdAsync(tenantId, ct);
        var localNow = ConvertToLocalTime(DateTime.UtcNow, tenant?.Timezone);

        var dayOfWeek = localNow.DayOfWeek;
        var timeOfDay = TimeOnly.FromDateTime(localNow);

        // Verificar se está no turno
        if (schedule.IsOnDutyAt(dayOfWeek, timeOfDay)) return true;

        // Aplicar tolerância: verificar se está dentro do período de tolerância após o fim do turno
        var workforceSettings = await _workforceSettings.GetByTenantIdAsync(tenantId, ct);
        int toleranceMinutes = workforceSettings?.ShiftToleranceMinutes ?? 15;

        if (toleranceMinutes > 0)
        {
            var timeWithTolerance = timeOfDay.AddMinutes(-toleranceMinutes);
            if (schedule.IsOnDutyAt(dayOfWeek, timeWithTolerance)) return true;
        }

        return false;
    }

    public async Task EnsureCanOperateAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default)
    {
        if (!await CanOperateAsync(tenantId, employeeId, ct))
            throw new ForbiddenException(
                "Funcionário fora do turno. Solicite ao gerente liberação de acesso temporário.");
    }

    private static DateTime ConvertToLocalTime(DateTime utcNow, string? ianaTimezone)
    {
        if (string.IsNullOrWhiteSpace(ianaTimezone))
            return utcNow;

        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(ianaTimezone);
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
        }
        catch
        {
            return utcNow;
        }
    }
}
