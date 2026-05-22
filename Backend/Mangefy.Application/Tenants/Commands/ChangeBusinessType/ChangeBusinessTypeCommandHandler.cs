using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.BusinessTypes.Repositories;
using Mangefy.Domain.Tenants;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Tenants.Commands.ChangeBusinessType;

public sealed class ChangeBusinessTypeCommandHandler : IRequestHandler<ChangeBusinessTypeCommand>
{
    private readonly ITenantRepository _tenants;
    private readonly IBusinessTypeRepository _businessTypes;
    private readonly IUnitOfWork _uow;

    public ChangeBusinessTypeCommandHandler(
        ITenantRepository tenants,
        IBusinessTypeRepository businessTypes,
        IUnitOfWork uow)
    {
        _tenants = tenants;
        _businessTypes = businessTypes;
        _uow = uow;
    }

    public async Task Handle(ChangeBusinessTypeCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenants.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), request.TenantId);

        var bt = await _businessTypes.GetByIdAsync(request.BusinessTypeId, cancellationToken)
            ?? throw new NotFoundException("BusinessType", request.BusinessTypeId);

        tenant.ChangeBusinessType(bt.Id);
        await _tenants.UpdateAsync(tenant, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
