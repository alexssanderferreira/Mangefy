using Mangefy.Domain.Employees;
using Mangefy.Domain.Employees.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class ActivationTokenRepository : IActivationTokenRepository
{
    private readonly MangefyDbContext _context;
    public ActivationTokenRepository(MangefyDbContext context) => _context = context;

    public Task<ActivationToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => _context.ActivationTokens.FirstOrDefaultAsync(x => x.Token == token, ct);

    public async Task AddAsync(ActivationToken token, CancellationToken ct = default)
        => await _context.ActivationTokens.AddAsync(token, ct);

    public Task UpdateAsync(ActivationToken token, CancellationToken ct = default)
    {
        _context.ActivationTokens.Update(token);
        return Task.CompletedTask;
    }
}
