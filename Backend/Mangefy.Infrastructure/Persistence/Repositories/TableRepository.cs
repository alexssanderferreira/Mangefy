using Mangefy.Domain.Tables;
using Mangefy.Domain.Tables.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class TableRepository : ITableRepository
{
    private readonly MangefyDbContext _context;
    public TableRepository(MangefyDbContext context) => _context = context;

    public Task<Table?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Tables.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Table>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.Tables.Where(x => x.TenantId == tenantId).ToListAsync(ct);

    public Task<Table?> GetByNumberAsync(Guid tenantId, string number, CancellationToken ct = default)
        => _context.Tables.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Number == number, ct);

    public Task<bool> ExistsByNumberAsync(Guid tenantId, string number, CancellationToken ct = default)
        => _context.Tables.AnyAsync(x => x.TenantId == tenantId && x.Number == number, ct);

    public Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _context.Tables.CountAsync(x => x.TenantId == tenantId, ct);

    public async Task AddAsync(Table table, CancellationToken ct = default)
        => await _context.Tables.AddAsync(table, ct);

    public Task UpdateAsync(Table table, CancellationToken ct = default)
    {
        _context.Tables.Update(table);
        return Task.CompletedTask;
    }
}
