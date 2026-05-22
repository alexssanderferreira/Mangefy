using Mangefy.Domain.Settings;
using Mangefy.Domain.Settings.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class ReservationSettingsRepository : IReservationSettingsRepository
{
    private readonly MangefyDbContext _context;

    public ReservationSettingsRepository(MangefyDbContext context)
        => _context = context;

    public Task<ReservationSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => _context.ReservationSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

    public async Task AddAsync(ReservationSettings settings, CancellationToken cancellationToken = default)
        => await _context.ReservationSettings.AddAsync(settings, cancellationToken);

    public Task UpdateAsync(ReservationSettings settings, CancellationToken cancellationToken = default)
    {
        _context.ReservationSettings.Update(settings);
        return Task.CompletedTask;
    }
}
