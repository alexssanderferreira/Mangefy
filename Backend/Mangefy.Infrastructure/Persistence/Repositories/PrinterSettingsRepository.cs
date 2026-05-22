using Mangefy.Domain.Settings;
using Mangefy.Domain.Settings.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class PrinterSettingsRepository : IPrinterSettingsRepository
{
    private readonly MangefyDbContext _context;
    public PrinterSettingsRepository(MangefyDbContext context) => _context = context;

    public Task<PrinterSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => _context.PrinterSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

    public async Task AddAsync(PrinterSettings settings, CancellationToken ct = default)
        => await _context.PrinterSettings.AddAsync(settings, ct);

    public Task UpdateAsync(PrinterSettings settings, CancellationToken ct = default)
    {
        if (_context.Entry(settings).State == EntityState.Detached)
            _context.PrinterSettings.Update(settings);
        return Task.CompletedTask;
    }
}
