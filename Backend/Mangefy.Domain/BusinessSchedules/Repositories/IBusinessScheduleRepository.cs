namespace Mangefy.Domain.BusinessSchedules.Repositories;

public interface IBusinessScheduleRepository
{
    Task<BusinessSchedule?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(BusinessSchedule schedule, CancellationToken cancellationToken = default);
    Task UpdateAsync(BusinessSchedule schedule, CancellationToken cancellationToken = default);
}
