using Mangefy.Domain.Devices.Repositories;
using MediatR;

namespace Mangefy.Application.Devices.Queries.GetDevices;

public sealed class GetDevicesQueryHandler : IRequestHandler<GetDevicesQuery, IReadOnlyList<DeviceDto>>
{
    private readonly IDeviceRepository _devices;

    public GetDevicesQueryHandler(IDeviceRepository devices) => _devices = devices;

    public async Task<IReadOnlyList<DeviceDto>> Handle(GetDevicesQuery request, CancellationToken cancellationToken)
    {
        var list = await _devices.GetByTenantAsync(request.TenantId, cancellationToken);
        return list.Select(d => new DeviceDto(
            d.Id, d.Name, d.Type.ToString(), d.PublicIdentifier,
            d.Status.ToString(), d.LastSeenAt, d.CreatedAt)).ToList();
    }
}
