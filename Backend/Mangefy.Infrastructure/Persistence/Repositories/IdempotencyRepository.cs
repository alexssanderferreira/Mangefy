using Mangefy.Domain.Idempotency;
using Mangefy.Domain.Idempotency.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class IdempotencyRepository : IIdempotencyRepository
{
    private readonly MangefyDbContext _context;
    public IdempotencyRepository(MangefyDbContext context) => _context = context;

    public Task<IdempotencyEntry?> GetAsync(Guid tenantId, Guid commandId, CancellationToken ct = default)
        => _context.IdempotencyEntries
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.CommandId == commandId, ct);

    public async Task AddAsync(IdempotencyEntry entry, CancellationToken ct = default)
        => await _context.IdempotencyEntries.AddAsync(entry, ct);
}
