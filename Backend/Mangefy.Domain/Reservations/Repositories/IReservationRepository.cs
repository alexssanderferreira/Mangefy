namespace Mangefy.Domain.Reservations.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> GetByDateAsync(Guid tenantId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> GetByTableAsync(Guid tenantId, Guid tableId, DateOnly date, CancellationToken ct = default);
    Task AddAsync(Reservation reservation, CancellationToken ct = default);
    Task UpdateAsync(Reservation reservation, CancellationToken ct = default);
}
