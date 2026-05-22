namespace Mangefy.Domain.Plans.Repositories;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Plan>> GetAllActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Plan>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Plan plan, CancellationToken ct = default);
    Task UpdateAsync(Plan plan, CancellationToken ct = default);
    Task DeleteAsync(Plan plan, CancellationToken ct = default);
}
