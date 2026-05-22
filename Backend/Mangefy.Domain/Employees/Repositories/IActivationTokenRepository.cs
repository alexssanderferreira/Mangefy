namespace Mangefy.Domain.Employees.Repositories;

public interface IActivationTokenRepository
{
    Task<ActivationToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task AddAsync(ActivationToken token, CancellationToken ct = default);
    Task UpdateAsync(ActivationToken token, CancellationToken ct = default);
}
