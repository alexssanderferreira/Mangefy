using MediatR;

namespace Mangefy.Application.Devices.Commands.DeactivateDevice;

public sealed record DeactivateDeviceCommand(Guid TenantId, Guid DeviceId) : IRequest;
