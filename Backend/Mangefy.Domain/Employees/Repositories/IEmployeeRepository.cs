namespace Mangefy.Domain.Employees.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> GetAllByEmailAsync(string email, CancellationToken ct = default);
    Task<Employee?> GetByEmailInTenantAsync(Guid tenantId, string email, CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> GetByRoleAsync(Guid tenantRoleId, CancellationToken ct = default);
    Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByEmailInTenantAsync(Guid tenantId, string email, CancellationToken ct = default);
    Task AddAsync(Employee employee, CancellationToken ct = default);
    Task UpdateAsync(Employee employee, CancellationToken ct = default);
}
