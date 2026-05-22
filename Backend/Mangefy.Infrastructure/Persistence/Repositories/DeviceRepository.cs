using Mangefy.Domain.Devices;
using Mangefy.Domain.Devices.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class DeviceRepository : IDeviceRepository
{
    private readonly MangefyDbContext _context;
    public DeviceRepository(MangefyDbContext context) => _context = context;

    public Task<Device?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Devices.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Device?> GetByPublicIdentifierAsync(Guid tenantId, string publicIdentifier, CancellationToken ct = default)
        => _context.Devices.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PublicIdentifier == publicIdentifier, ct);

    public async Task<IReadOnlyList<Device>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.Devices.Where(x => x.TenantId == tenantId).ToListAsync(ct);

    public async Task AddAsync(Device device, CancellationToken ct = default)
        => await _context.Devices.AddAsync(device, ct);

    public Task UpdateAsync(Device device, CancellationToken ct = default)
    {
        _context.Devices.Update(device);
        return Task.CompletedTask;
    }
}
