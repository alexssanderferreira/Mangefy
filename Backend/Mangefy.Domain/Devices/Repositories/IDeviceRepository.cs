namespace Mangefy.Domain.Devices.Repositories;

public interface IDeviceRepository
{
    Task<Device?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Device?> GetByPublicIdentifierAsync(Guid tenantId, string publicIdentifier, CancellationToken ct = default);
    Task<IReadOnlyList<Device>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Device device, CancellationToken ct = default);
    Task UpdateAsync(Device device, CancellationToken ct = default);
}
