using Mangefy.Domain.Platform.SupplierCategories;
using Mangefy.Domain.Platform.SupplierCategories.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class SupplierCategoryRepository : ISupplierCategoryRepository
{
    private readonly MangefyDbContext _context;
    public SupplierCategoryRepository(MangefyDbContext context) => _context = context;

    public Task<SupplierCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.SupplierCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<SupplierCategory>> GetAllGlobalAsync(CancellationToken cancellationToken = default)
        => await _context.SupplierCategories
            .Where(c => c.TenantId == null)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SupplierCategory>> GetGlobalAsync(CancellationToken cancellationToken = default)
        => await _context.SupplierCategories
            .Where(c => c.TenantId == null && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SupplierCategory>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => await _context.SupplierCategories
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(SupplierCategory category, CancellationToken cancellationToken = default)
        => await _context.SupplierCategories.AddAsync(category, cancellationToken);

    public Task UpdateAsync(SupplierCategory category, CancellationToken cancellationToken = default)
    {
        _context.SupplierCategories.Update(category);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(SupplierCategory category, CancellationToken cancellationToken = default)
    {
        _context.SupplierCategories.Remove(category);
        return Task.CompletedTask;
    }
}
