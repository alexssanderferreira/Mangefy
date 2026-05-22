using MediatR;

namespace Mangefy.Application.Devices.Queries.GetDevices;

public sealed record GetDevicesQuery(Guid TenantId) : IRequest<IReadOnlyList<DeviceDto>>;

public sealed record DeviceDto(
    Guid Id,
    string Name,
    string Type,
    string PublicIdentifier,
    string Status,
    DateTime? LastSeenAt,
    DateTime CreatedAt);
