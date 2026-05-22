using Mangefy.Domain.OperationalSessions;
using Mangefy.Domain.OperationalSessions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class OperationalSessionRepository : IOperationalSessionRepository
{
    private readonly MangefyDbContext _context;
    public OperationalSessionRepository(MangefyDbContext context) => _context = context;

    public Task<OperationalSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.OperationalSessions.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<OperationalSession?> GetActiveByEmployeeAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default)
        => _context.OperationalSessions
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EmployeeId == employeeId && x.Status == OperationalSessionStatus.Active, ct);

    public async Task<IReadOnlyList<OperationalSession>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.OperationalSessions
            .Where(x => x.TenantId == tenantId && x.Status == OperationalSessionStatus.Active)
            .ToListAsync(ct);

    public async Task AddAsync(OperationalSession session, CancellationToken ct = default)
        => await _context.OperationalSessions.AddAsync(session, ct);

    public Task UpdateAsync(OperationalSession session, CancellationToken ct = default)
    {
        _context.OperationalSessions.Update(session);
        return Task.CompletedTask;
    }
}
