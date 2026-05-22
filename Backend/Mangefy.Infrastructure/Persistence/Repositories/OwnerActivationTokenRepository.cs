using Mangefy.Domain.Owners;
using Mangefy.Domain.Owners.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class OwnerActivationTokenRepository : IOwnerActivationTokenRepository
{
    private readonly MangefyDbContext _context;
    public OwnerActivationTokenRepository(MangefyDbContext context) => _context = context;

    public Task<OwnerActivationToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => _context.OwnerActivationTokens.FirstOrDefaultAsync(x => x.Token == token, ct);

    public Task AddAsync(OwnerActivationToken token, CancellationToken ct = default)
    {
        _context.OwnerActivationTokens.Add(token);
        return Task.CompletedTask;
    }
}
