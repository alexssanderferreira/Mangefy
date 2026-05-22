using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Devices;
using Mangefy.Domain.Devices.Repositories;
using MediatR;

namespace Mangefy.Application.Devices.Commands.RegisterDevice;

public sealed class RegisterDeviceCommandHandler : IRequestHandler<RegisterDeviceCommand, Guid>
{
    private readonly IDeviceRepository _devices;
    private readonly IUnitOfWork _uow;

    public RegisterDeviceCommandHandler(IDeviceRepository devices, IUnitOfWork uow)
    {
        _devices = devices;
        _uow = uow;
    }

    public async Task<Guid> Handle(RegisterDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = Device.Register(request.TenantId, request.Name, request.Type, request.PublicIdentifier);
        await _devices.AddAsync(device, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return device.Id;
    }
}
