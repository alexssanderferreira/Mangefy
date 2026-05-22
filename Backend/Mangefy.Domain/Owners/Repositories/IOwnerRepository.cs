namespace Mangefy.Domain.Owners.Repositories;

public interface IOwnerRepository
{
    Task<Owner?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Owner?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<(IReadOnlyList<Owner> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Owner owner, CancellationToken ct = default);
    Task UpdateAsync(Owner owner, CancellationToken ct = default);
}
