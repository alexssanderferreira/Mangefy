using Mangefy.Domain.BusinessSchedules;
using Mangefy.Domain.BusinessSchedules.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class BusinessScheduleRepository : IBusinessScheduleRepository
{
    private readonly MangefyDbContext _context;
    public BusinessScheduleRepository(MangefyDbContext context) => _context = context;

    public Task<BusinessSchedule?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => _context.BusinessSchedules.FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

    public async Task AddAsync(BusinessSchedule schedule, CancellationToken ct = default)
        => await _context.BusinessSchedules.AddAsync(schedule, ct);

    public Task UpdateAsync(BusinessSchedule schedule, CancellationToken ct = default)
    {
        if (_context.Entry(schedule).State == EntityState.Detached)
            _context.BusinessSchedules.Update(schedule);
        return Task.CompletedTask;
    }
}
