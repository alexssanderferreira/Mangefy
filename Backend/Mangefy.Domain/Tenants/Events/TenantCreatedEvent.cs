using Mangefy.Domain.Common;

namespace Mangefy.Domain.Tenants.Events;

public sealed class TenantCreatedEvent(Guid tenantId, string name, string slug, Guid businessTypeId) : DomainEvent
{
    public Guid TenantId { get; } = tenantId;
    public string Name { get; } = name;
    public string Slug { get; } = slug;
    public Guid BusinessTypeId { get; } = businessTypeId;
}
