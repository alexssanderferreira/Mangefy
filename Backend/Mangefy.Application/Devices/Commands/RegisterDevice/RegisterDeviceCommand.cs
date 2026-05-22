using Mangefy.Domain.Devices;
using MediatR;

namespace Mangefy.Application.Devices.Commands.RegisterDevice;

public sealed record RegisterDeviceCommand(
    Guid TenantId,
    string Name,
    DeviceType Type,
    string PublicIdentifier
) : IRequest<Guid>;
