using Mangefy.Domain.Owners;
using Mangefy.Domain.Owners.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class OwnerRepository : IOwnerRepository
{
    private readonly MangefyDbContext _context;
    public OwnerRepository(MangefyDbContext context) => _context = context;

    public Task<Owner?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Owners.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Owner?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _context.Owners.FirstOrDefaultAsync(x => x.Email.Value == email, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => _context.Owners.AnyAsync(x => x.Email.Value == email, ct);

    public async Task<(IReadOnlyList<Owner> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Owners.OrderByDescending(x => x.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public Task AddAsync(Owner owner, CancellationToken ct = default)
    {
        _context.Owners.Add(owner);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Owner owner, CancellationToken ct = default)
    {
        _context.Owners.Update(owner);
        return Task.CompletedTask;
    }
}
