namespace Mangefy.Domain.Platform.Features.Repositories;

public interface IPlanFeatureSetRepository
{
    Task<PlanFeatureSet?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PlanFeatureSet?> GetByPlanAndBusinessTypeAsync(Guid planId, Guid businessTypeId, CancellationToken ct = default);
    Task<IReadOnlyList<PlanFeatureSet>> GetByPlanAsync(Guid planId, CancellationToken ct = default);
    Task<IReadOnlyList<PlanFeatureSet>> GetByBusinessTypeAsync(Guid businessTypeId, CancellationToken ct = default);
    Task AddAsync(PlanFeatureSet planFeatureSet, CancellationToken ct = default);
    Task UpdateAsync(PlanFeatureSet planFeatureSet, CancellationToken ct = default);
}
