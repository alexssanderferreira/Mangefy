using Mangefy.Domain.Roles;

namespace Mangefy.Application.Roles.Queries.GetRolesByTenant;

public sealed record TenantRoleDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsOwnerRole,
    bool IsFromTemplate,
    bool IsActive,
    IReadOnlyCollection<string> Permissions
)
{
    public static TenantRoleDto FromDomain(TenantRole r) => new(
        r.Id,
        r.Name,
        r.Description,
        r.IsOwnerRole,
        r.IsFromTemplate,
        r.IsActive,
        r.Permissions);
}
