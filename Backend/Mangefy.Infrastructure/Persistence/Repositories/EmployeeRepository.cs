using Mangefy.Domain.Employees;
using Mangefy.Domain.Employees.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly MangefyDbContext _context;
    public EmployeeRepository(MangefyDbContext context) => _context = context;

    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Employees.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _context.Employees.FirstOrDefaultAsync(x => x.Email.Value == email, ct);

    public async Task<IReadOnlyList<Employee>> GetAllByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Employees.Where(x => x.Email.Value == email).ToListAsync(ct);

public async Task<IReadOnlyList<Employee>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.Employees.Where(x => x.TenantId == tenantId).ToListAsync(ct);

    public async Task<IReadOnlyList<Employee>> GetByRoleAsync(Guid tenantRoleId, CancellationToken ct = default)
        => await _context.Employees.Where(x => x.TenantRoleId == tenantRoleId).ToListAsync(ct);

    public Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _context.Employees.CountAsync(x => x.TenantId == tenantId, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => _context.Employees.AnyAsync(x => x.Email.Value == email, ct);

    public Task<bool> ExistsByEmailInTenantAsync(Guid tenantId, string email, CancellationToken ct = default)
        => _context.Employees.AnyAsync(x => x.TenantId == tenantId && x.Email.Value == email, ct);

    public Task<Employee?> GetByEmailInTenantAsync(Guid tenantId, string email, CancellationToken ct = default)
        => _context.Employees.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Email.Value == email, ct);

    public async Task AddAsync(Employee employee, CancellationToken ct = default)
        => await _context.Employees.AddAsync(employee, ct);

    public Task UpdateAsync(Employee employee, CancellationToken ct = default)
    {
        _context.Employees.Update(employee);
        return Task.CompletedTask;
    }
}
