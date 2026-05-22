namespace Mangefy.Domain.Idempotency.Repositories;

public interface IIdempotencyRepository
{
    Task<IdempotencyEntry?> GetAsync(Guid tenantId, Guid commandId, CancellationToken ct = default);
    Task AddAsync(IdempotencyEntry entry, CancellationToken ct = default);
}
