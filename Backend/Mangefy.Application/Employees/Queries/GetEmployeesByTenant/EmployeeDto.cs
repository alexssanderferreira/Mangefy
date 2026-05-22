using Mangefy.Domain.Employees;

namespace Mangefy.Application.Employees.Queries.GetEmployeesByTenant;

public sealed record EmployeeDto(
    Guid Id,
    string Name,
    string Email,
    Guid TenantRoleId,
    string Status,
    DateTime? LastLoginAt,
    DateTime CreatedAt
)
{
    public static EmployeeDto FromDomain(Employee e) => new(
        e.Id,
        e.Name,
        e.Email.Value,
        e.TenantRoleId,
        e.Status.ToString(),
        e.LastLoginAt,
        e.CreatedAt);
}
