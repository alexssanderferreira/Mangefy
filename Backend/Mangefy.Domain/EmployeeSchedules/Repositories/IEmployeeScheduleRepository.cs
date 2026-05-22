namespace Mangefy.Domain.EmployeeSchedules.Repositories;

public interface IEmployeeScheduleRepository
{
    Task<EmployeeSchedule?> GetByEmployeeIdAsync(Guid tenantId, Guid employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeSchedule>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(EmployeeSchedule schedule, CancellationToken cancellationToken = default);
    Task UpdateAsync(EmployeeSchedule schedule, CancellationToken cancellationToken = default);
}
