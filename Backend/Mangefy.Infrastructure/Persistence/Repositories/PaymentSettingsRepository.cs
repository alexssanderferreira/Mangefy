using Mangefy.Domain.Settings;
using Mangefy.Domain.Settings.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class PaymentSettingsRepository : IPaymentSettingsRepository
{
    private readonly MangefyDbContext _context;
    public PaymentSettingsRepository(MangefyDbContext context) => _context = context;

    public Task<PaymentSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => _context.PaymentSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

    public async Task AddAsync(PaymentSettings settings, CancellationToken ct = default)
        => await _context.PaymentSettings.AddAsync(settings, ct);

    public Task UpdateAsync(PaymentSettings settings, CancellationToken ct = default)
    {
        _context.PaymentSettings.Update(settings);
        return Task.CompletedTask;
    }
}
