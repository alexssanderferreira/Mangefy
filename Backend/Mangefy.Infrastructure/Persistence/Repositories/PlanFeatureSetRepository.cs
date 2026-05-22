using Mangefy.Domain.Platform.Features;
using Mangefy.Domain.Platform.Features.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class PlanFeatureSetRepository : IPlanFeatureSetRepository
{
    private readonly MangefyDbContext _context;

    public PlanFeatureSetRepository(MangefyDbContext context) => _context = context;

    public Task<PlanFeatureSet?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.PlanFeatureSets.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<PlanFeatureSet?> GetByPlanAndBusinessTypeAsync(Guid planId, Guid businessTypeId, CancellationToken ct = default)
        => _context.PlanFeatureSets.FirstOrDefaultAsync(x => x.PlanId == planId && x.BusinessTypeId == businessTypeId, ct);

    public async Task<IReadOnlyList<PlanFeatureSet>> GetByPlanAsync(Guid planId, CancellationToken ct = default)
        => await _context.PlanFeatureSets.Where(x => x.PlanId == planId).ToListAsync(ct);

    public async Task<IReadOnlyList<PlanFeatureSet>> GetByBusinessTypeAsync(Guid businessTypeId, CancellationToken ct = default)
        => await _context.PlanFeatureSets.Where(x => x.BusinessTypeId == businessTypeId).ToListAsync(ct);

    public async Task AddAsync(PlanFeatureSet planFeatureSet, CancellationToken ct = default)
        => await _context.PlanFeatureSets.AddAsync(planFeatureSet, ct);

    public Task UpdateAsync(PlanFeatureSet planFeatureSet, CancellationToken ct = default)
    {
        _context.PlanFeatureSets.Update(planFeatureSet);
        return Task.CompletedTask;
    }
}
