using Mangefy.Domain.Reservations;
using Mangefy.Domain.Reservations.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class ReservationRepository : IReservationRepository
{
    private readonly MangefyDbContext _context;
    public ReservationRepository(MangefyDbContext context) => _context = context;

    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Reservations.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Reservation>> GetByDateAsync(Guid tenantId, DateOnly date, CancellationToken ct = default)
        => await _context.Reservations.Where(x => x.TenantId == tenantId && x.Date == date).ToListAsync(ct);

    public async Task<IReadOnlyList<Reservation>> GetByTableAsync(Guid tenantId, Guid tableId, DateOnly date, CancellationToken ct = default)
        => await _context.Reservations.Where(x => x.TenantId == tenantId && x.TableId == tableId && x.Date == date).ToListAsync(ct);

    public async Task AddAsync(Reservation reservation, CancellationToken ct = default)
        => await _context.Reservations.AddAsync(reservation, ct);

    public Task UpdateAsync(Reservation reservation, CancellationToken ct = default)
    {
        _context.Reservations.Update(reservation);
        return Task.CompletedTask;
    }
}
