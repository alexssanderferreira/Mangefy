namespace Mangefy.Domain.Stock.Repositories;

public interface IStockRepository
{
    Task<Stock?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Stock stock, CancellationToken cancellationToken = default);
    Task UpdateAsync(Stock stock, CancellationToken cancellationToken = default);
}
