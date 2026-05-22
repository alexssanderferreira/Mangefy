using Mangefy.Domain.Plans;
using Mangefy.Domain.Plans.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class PlanRepository : IPlanRepository
{
    private readonly MangefyDbContext _context;
    public PlanRepository(MangefyDbContext context) => _context = context;

    public Task<Plan?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Plans.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Plan>> GetAllActiveAsync(CancellationToken ct = default)
        => await _context.Plans.Where(x => x.Status == PlanStatus.Active).ToListAsync(ct);

    public async Task<IReadOnlyList<Plan>> GetAllAsync(CancellationToken ct = default)
        => await _context.Plans.OrderBy(x => x.Name).ToListAsync(ct);

    public async Task AddAsync(Plan plan, CancellationToken ct = default)
        => await _context.Plans.AddAsync(plan, ct);

    public Task UpdateAsync(Plan plan, CancellationToken ct = default)
    {
        _context.Plans.Update(plan);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Plan plan, CancellationToken ct = default)
    {
        _context.Plans.Remove(plan);
        return Task.CompletedTask;
    }
}
