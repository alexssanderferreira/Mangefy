using Mangefy.Domain.Common;

namespace Mangefy.Domain.Roles.Events;

public sealed class TenantRoleCreatedEvent(Guid roleId, Guid tenantId, string name) : DomainEvent
{
    public Guid RoleId { get; } = roleId;
    public Guid TenantId { get; } = tenantId;
    public string Name { get; } = name;
}
