using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Devices.Repositories;
using MediatR;

namespace Mangefy.Application.Devices.Commands.DeactivateDevice;

public sealed class DeactivateDeviceCommandHandler : IRequestHandler<DeactivateDeviceCommand>
{
    private readonly IDeviceRepository _devices;
    private readonly IUnitOfWork _uow;

    public DeactivateDeviceCommandHandler(IDeviceRepository devices, IUnitOfWork uow)
    {
        _devices = devices;
        _uow = uow;
    }

    public async Task Handle(DeactivateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _devices.GetByIdAsync(request.DeviceId, cancellationToken)
            ?? throw new NotFoundException("Device", request.DeviceId);

        if (device.TenantId != request.TenantId)
            throw new ForbiddenException();

        device.Deactivate();
        await _devices.UpdateAsync(device, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
