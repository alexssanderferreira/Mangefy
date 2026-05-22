namespace Mangefy.Domain.PrintJobs.Repositories;

public interface IPrintJobRepository
{
    Task<PrintJob?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PrintJob>> GetPendingByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(PrintJob job, CancellationToken ct = default);
    Task UpdateAsync(PrintJob job, CancellationToken ct = default);
}
