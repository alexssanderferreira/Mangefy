namespace Mangefy.Domain.Owners.Repositories;

public interface IOwnerActivationTokenRepository
{
    Task<OwnerActivationToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task AddAsync(OwnerActivationToken token, CancellationToken ct = default);
}
