using Mangefy.Domain.Common;

namespace Mangefy.Domain.Tenants.Events;

public sealed class TenantCancelledEvent(Guid tenantId, string name) : DomainEvent
{
    public Guid TenantId { get; } = tenantId;
    public string Name { get; } = name;
}
