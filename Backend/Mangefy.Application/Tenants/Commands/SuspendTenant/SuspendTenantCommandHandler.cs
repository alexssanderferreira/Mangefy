using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Tenants;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Tenants.Commands.SuspendTenant;

public sealed class SuspendTenantCommandHandler : IRequestHandler<SuspendTenantCommand>
{
    private readonly ITenantRepository _tenants;
    private readonly IUnitOfWork _uow;

    public SuspendTenantCommandHandler(ITenantRepository tenants, IUnitOfWork uow)
    {
        _tenants = tenants;
        _uow = uow;
    }

    public async Task Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenants.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), request.TenantId);

        tenant.Suspend();
        await _tenants.UpdateAsync(tenant, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
