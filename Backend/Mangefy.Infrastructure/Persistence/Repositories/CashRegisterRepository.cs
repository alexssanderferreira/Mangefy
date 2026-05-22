using Mangefy.Domain.DailyCash;
using Mangefy.Domain.DailyCash.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class CashRegisterRepository : ICashRegisterRepository
{
    private readonly MangefyDbContext _context;
    public CashRegisterRepository(MangefyDbContext context) => _context = context;

    public Task<CashRegister?> GetOpenByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _context.CashRegisters.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Status == CashRegisterStatus.Open, ct);

    public Task<CashRegister?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.CashRegisters.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<CashRegister>> GetHistoryByTenantAsync(Guid tenantId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var fromDt = from.ToDateTime(TimeOnly.MinValue);
        var toDt = to.ToDateTime(TimeOnly.MaxValue);
        return await _context.CashRegisters
            .Where(x => x.TenantId == tenantId && x.OpenedAt >= fromDt && x.OpenedAt <= toDt)
            .OrderByDescending(x => x.OpenedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(CashRegister register, CancellationToken ct = default)
        => await _context.CashRegisters.AddAsync(register, ct);

    public Task UpdateAsync(CashRegister register, CancellationToken ct = default)
    {
        _context.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            if (_context.Entry(register).State == EntityState.Detached)
            {
                _context.CashRegisters.Update(register);
                return Task.CompletedTask;
            }

            foreach (var w in register.Withdrawals)
            {
                if (_context.Entry(w).State == EntityState.Detached)
                    _context.Entry(w).State = EntityState.Added;
            }

            foreach (var s in register.Supplies)
            {
                if (_context.Entry(s).State == EntityState.Detached)
                    _context.Entry(s).State = EntityState.Added;
            }
        }
        finally
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        return Task.CompletedTask;
    }
}
