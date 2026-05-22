using Mangefy.Domain.Fiscal;
using Mangefy.Domain.Fiscal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class FiscalDocumentRepository : IFiscalDocumentRepository
{
    private readonly MangefyDbContext _context;

    public FiscalDocumentRepository(MangefyDbContext context) => _context = context;

    public Task<FiscalDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.FiscalDocuments.FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<FiscalDocument?> GetByTabIdAsync(Guid tabId, CancellationToken ct = default)
        => _context.FiscalDocuments.FirstOrDefaultAsync(d => d.TabId == tabId, ct);

    public async Task<IReadOnlyList<FiscalDocument>> GetByTenantAsync(
        Guid tenantId, DateTime from, DateTime to, CancellationToken ct = default)
        => await _context.FiscalDocuments
            .Where(d => d.TenantId == tenantId && d.CreatedAt >= from && d.CreatedAt <= to)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(FiscalDocument document, CancellationToken ct = default)
        => await _context.FiscalDocuments.AddAsync(document, ct);

    public Task UpdateAsync(FiscalDocument document, CancellationToken ct = default)
    {
        _context.FiscalDocuments.Update(document);
        return Task.CompletedTask;
    }
}
