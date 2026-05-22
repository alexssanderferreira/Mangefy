namespace Mangefy.Domain.Platform.BusinessTypes.Repositories;

public interface IBusinessTypeRepository
{
    Task<BusinessType?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<BusinessType>> GetAllActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<BusinessType>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(BusinessType businessType, CancellationToken ct = default);
    Task UpdateAsync(BusinessType businessType, CancellationToken ct = default);
    Task DeleteAsync(BusinessType businessType, CancellationToken ct = default);
}
