using Mangefy.Domain.Settings;
using Mangefy.Domain.Settings.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class IntegrationSettingsRepository : IIntegrationSettingsRepository
{
    private readonly MangefyDbContext _context;
    public IntegrationSettingsRepository(MangefyDbContext context) => _context = context;

    public Task<IntegrationSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => _context.IntegrationSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

    public async Task AddAsync(IntegrationSettings settings, CancellationToken ct = default)
        => await _context.IntegrationSettings.AddAsync(settings, ct);

    public Task UpdateAsync(IntegrationSettings settings, CancellationToken ct = default)
    {
        _context.IntegrationSettings.Update(settings);
        return Task.CompletedTask;
    }
}
