using Mangefy.Domain.PrintJobs;
using Mangefy.Domain.PrintJobs.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class PrintJobRepository : IPrintJobRepository
{
    private readonly MangefyDbContext _context;
    public PrintJobRepository(MangefyDbContext context) => _context = context;

    public Task<PrintJob?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.PrintJobs.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<PrintJob>> GetPendingByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.PrintJobs
            .Where(x => x.TenantId == tenantId && x.Status == PrintJobStatus.Pending)
            .ToListAsync(ct);

    public async Task AddAsync(PrintJob job, CancellationToken ct = default)
        => await _context.PrintJobs.AddAsync(job, ct);

    public Task UpdateAsync(PrintJob job, CancellationToken ct = default)
    {
        _context.PrintJobs.Update(job);
        return Task.CompletedTask;
    }
}
