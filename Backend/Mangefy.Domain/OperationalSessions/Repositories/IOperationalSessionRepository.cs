namespace Mangefy.Domain.OperationalSessions.Repositories;

public interface IOperationalSessionRepository
{
    Task<OperationalSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<OperationalSession?> GetActiveByEmployeeAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<OperationalSession>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(OperationalSession session, CancellationToken ct = default);
    Task UpdateAsync(OperationalSession session, CancellationToken ct = default);
}
