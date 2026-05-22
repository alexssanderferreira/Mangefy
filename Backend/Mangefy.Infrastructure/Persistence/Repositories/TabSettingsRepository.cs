using Mangefy.Domain.Settings;
using Mangefy.Domain.Settings.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class TabSettingsRepository : ITabSettingsRepository
{
    private readonly MangefyDbContext _context;
    public TabSettingsRepository(MangefyDbContext context) => _context = context;

    public Task<TabSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => _context.TabSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

    public async Task AddAsync(TabSettings settings, CancellationToken ct = default)
        => await _context.TabSettings.AddAsync(settings, ct);

    public Task UpdateAsync(TabSettings settings, CancellationToken ct = default)
    {
        _context.TabSettings.Update(settings);
        return Task.CompletedTask;
    }
}
