using Mangefy.Domain.EmployeeSchedules;
using Mangefy.Domain.EmployeeSchedules.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class EmployeeScheduleRepository : IEmployeeScheduleRepository
{
    private readonly MangefyDbContext _context;
    public EmployeeScheduleRepository(MangefyDbContext context) => _context = context;

    public Task<EmployeeSchedule?> GetByEmployeeIdAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default)
        => _context.EmployeeSchedules
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EmployeeId == employeeId, ct);

    public async Task<IReadOnlyList<EmployeeSchedule>> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.EmployeeSchedules
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(ct);

    public async Task AddAsync(EmployeeSchedule schedule, CancellationToken ct = default)
        => await _context.EmployeeSchedules.AddAsync(schedule, ct);

    public Task UpdateAsync(EmployeeSchedule schedule, CancellationToken ct = default)
    {
        if (_context.Entry(schedule).State == EntityState.Detached)
            _context.EmployeeSchedules.Update(schedule);
        return Task.CompletedTask;
    }
}
