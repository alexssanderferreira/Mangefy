using Mangefy.Domain.Common;

namespace Mangefy.Domain.Platform.BusinessTypes.Events;

public sealed class BusinessTypeCreatedEvent(Guid businessTypeId, string name) : DomainEvent
{
    public Guid BusinessTypeId { get; } = businessTypeId;
    public string Name { get; } = name;
}
