using Mangefy.Domain.Stock;
using Mangefy.Domain.Stock.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class StockRepository : IStockRepository
{
    private readonly MangefyDbContext _context;
    public StockRepository(MangefyDbContext context) => _context = context;

    public Task<Stock?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => _context.Stocks.FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

    public async Task AddAsync(Stock stock, CancellationToken cancellationToken = default)
        => await _context.Stocks.AddAsync(stock, cancellationToken);

    public Task UpdateAsync(Stock stock, CancellationToken cancellationToken = default)
    {
        _context.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            if (_context.Entry(stock).State == EntityState.Detached)
            {
                _context.Stocks.Update(stock);
                return Task.CompletedTask;
            }

            foreach (var item in stock.Items)
            {
                if (_context.Entry(item).State == EntityState.Detached)
                    _context.Entry(item).State = EntityState.Added;
            }

            foreach (var movement in stock.Movements)
            {
                if (_context.Entry(movement).State == EntityState.Detached)
                    _context.Entry(movement).State = EntityState.Added;
            }
        }
        finally
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        return Task.CompletedTask;
    }
}
