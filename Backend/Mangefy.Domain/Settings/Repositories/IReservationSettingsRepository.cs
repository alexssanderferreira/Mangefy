namespace Mangefy.Domain.Settings.Repositories;

public interface IReservationSettingsRepository
{
    Task<ReservationSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(ReservationSettings settings, CancellationToken cancellationToken = default);
    Task UpdateAsync(ReservationSettings settings, CancellationToken cancellationToken = default);
}
