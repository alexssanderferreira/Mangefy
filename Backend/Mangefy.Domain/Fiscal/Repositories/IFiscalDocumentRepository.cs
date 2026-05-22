namespace Mangefy.Domain.Fiscal.Repositories;

public interface IFiscalDocumentRepository
{
    Task<FiscalDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<FiscalDocument?> GetByTabIdAsync(Guid tabId, CancellationToken ct = default);
    Task<IReadOnlyList<FiscalDocument>> GetByTenantAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken ct = default);
    Task AddAsync(FiscalDocument document, CancellationToken ct = default);
    Task UpdateAsync(FiscalDocument document, CancellationToken ct = default);
}
