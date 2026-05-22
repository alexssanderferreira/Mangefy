using Mangefy.Domain.Settings;
using Mangefy.Domain.Settings.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class FiscalSettingsRepository : IFiscalSettingsRepository
{
    private readonly MangefyDbContext _context;
    public FiscalSettingsRepository(MangefyDbContext context) => _context = context;

    public Task<FiscalSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => _context.FiscalSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

    public async Task AddAsync(FiscalSettings settings, CancellationToken ct = default)
        => await _context.FiscalSettings.AddAsync(settings, ct);

    public Task UpdateAsync(FiscalSettings settings, CancellationToken ct = default)
    {
        _context.FiscalSettings.Update(settings);
        return Task.CompletedTask;
    }
}
