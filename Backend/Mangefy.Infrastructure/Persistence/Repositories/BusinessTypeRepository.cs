using Mangefy.Domain.Platform.BusinessTypes;
using Mangefy.Domain.Platform.BusinessTypes.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class BusinessTypeRepository : IBusinessTypeRepository
{
    private readonly MangefyDbContext _context;
    public BusinessTypeRepository(MangefyDbContext context) => _context = context;

    public Task<BusinessType?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.BusinessTypes
            .Include(x => x.RoleTemplates)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<BusinessType>> GetAllActiveAsync(CancellationToken ct = default)
        => await _context.BusinessTypes.Where(x => x.IsActive).ToListAsync(ct);

    public async Task<IReadOnlyList<BusinessType>> GetAllAsync(CancellationToken ct = default)
        => await _context.BusinessTypes.ToListAsync(ct);

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => _context.BusinessTypes.AnyAsync(x => x.Name == name, ct);

    public async Task AddAsync(BusinessType businessType, CancellationToken ct = default)
        => await _context.BusinessTypes.AddAsync(businessType, ct);

    public Task UpdateAsync(BusinessType businessType, CancellationToken ct = default)
    {
        // Disable auto-detect so Entry().State doesn't trigger DetectChanges,
        // which incorrectly classifies new owned entities as Modified instead of Detached.
        _context.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            if (_context.Entry(businessType).State == EntityState.Detached)
            {
                _context.BusinessTypes.Update(businessType);
                return Task.CompletedTask;
            }

            // New templates (Detached): mark as Added for INSERT
            foreach (var template in businessType.RoleTemplates)
            {
                if (_context.Entry(template).State == EntityState.Detached)
                    _context.Entry(template).State = EntityState.Added;
            }

            // Removed templates: any tracked template no longer in the collection must be Deleted
            var trackedTemplateIds = businessType.RoleTemplates.Select(t => t.Id).ToHashSet();
            var orphaned = _context.ChangeTracker
                .Entries<RoleTemplate>()
                .Where(e => e.Property<Guid>("BusinessTypeId").CurrentValue == businessType.Id
                            && !trackedTemplateIds.Contains(e.Entity.Id))
                .ToList();
            foreach (var orphan in orphaned)
                orphan.State = EntityState.Deleted;
        }
        finally
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(BusinessType businessType, CancellationToken ct = default)
    {
        _context.BusinessTypes.Remove(businessType);
        return Task.CompletedTask;
    }
}
