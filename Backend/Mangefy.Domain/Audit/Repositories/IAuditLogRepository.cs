namespace Mangefy.Domain.Audit.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLog>> GetByTenantAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken ct = default);
}
