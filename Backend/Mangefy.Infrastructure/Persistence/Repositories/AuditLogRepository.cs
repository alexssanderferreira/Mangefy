using Mangefy.Domain.Audit;
using Mangefy.Domain.Audit.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly MangefyDbContext _context;

    public AuditLogRepository(MangefyDbContext context) => _context = context;

    public async Task AddAsync(AuditLog log, CancellationToken ct = default)
        => await _context.AuditLogs.AddAsync(log, ct);

    public async Task<IReadOnlyList<AuditLog>> GetByTenantAsync(
        Guid tenantId, DateTime from, DateTime to, CancellationToken ct = default)
        => await _context.AuditLogs
            .Where(l => l.TenantId == tenantId && l.OccurredAt >= from && l.OccurredAt <= to)
            .OrderByDescending(l => l.OccurredAt)
            .ToListAsync(ct);
}
