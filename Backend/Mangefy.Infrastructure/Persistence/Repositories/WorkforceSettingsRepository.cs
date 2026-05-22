using Mangefy.Domain.Settings;
using Mangefy.Domain.Settings.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class WorkforceSettingsRepository : IWorkforceSettingsRepository
{
    private readonly MangefyDbContext _context;
    public WorkforceSettingsRepository(MangefyDbContext context) => _context = context;

    public Task<WorkforceSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => _context.WorkforceSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

    public async Task AddAsync(WorkforceSettings settings, CancellationToken ct = default)
        => await _context.WorkforceSettings.AddAsync(settings, ct);

    public Task UpdateAsync(WorkforceSettings settings, CancellationToken ct = default)
    {
        _context.WorkforceSettings.Update(settings);
        return Task.CompletedTask;
    }
}
