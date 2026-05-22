using Mangefy.Domain.Platform.Suppliers;
using Mangefy.Domain.Platform.Suppliers.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class PlatformSupplierRepository : IPlatformSupplierRepository
{
    private readonly MangefyDbContext _context;
    public PlatformSupplierRepository(MangefyDbContext context) => _context = context;

    public Task<PlatformSupplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.PlatformSuppliers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PlatformSupplier>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.PlatformSuppliers
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PlatformSupplier>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        => await _context.PlatformSuppliers
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PlatformSupplier>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => await _context.PlatformSuppliers
            .Where(s => s.IsActive && s.SupplierCategoryId == categoryId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(PlatformSupplier supplier, CancellationToken cancellationToken = default)
        => await _context.PlatformSuppliers.AddAsync(supplier, cancellationToken);

    public Task UpdateAsync(PlatformSupplier supplier, CancellationToken cancellationToken = default)
    {
        _context.PlatformSuppliers.Update(supplier);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(PlatformSupplier supplier, CancellationToken cancellationToken = default)
    {
        _context.PlatformSuppliers.Remove(supplier);
        return Task.CompletedTask;
    }
}
