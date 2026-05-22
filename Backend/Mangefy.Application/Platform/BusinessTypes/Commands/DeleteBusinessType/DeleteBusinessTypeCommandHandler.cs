using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Common;
using Mangefy.Domain.Platform.BusinessTypes;
using Mangefy.Domain.Platform.BusinessTypes.Repositories;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Commands.DeleteBusinessType;

public sealed class DeleteBusinessTypeCommandHandler : IRequestHandler<DeleteBusinessTypeCommand>
{
    private readonly IBusinessTypeRepository _businessTypes;
    private readonly ITenantRepository _tenants;
    private readonly IUnitOfWork _uow;

    public DeleteBusinessTypeCommandHandler(
        IBusinessTypeRepository businessTypes,
        ITenantRepository tenants,
        IUnitOfWork uow)
    {
        _businessTypes = businessTypes;
        _tenants = tenants;
        _uow = uow;
    }

    public async Task Handle(DeleteBusinessTypeCommand request, CancellationToken cancellationToken)
    {
        var bt = await _businessTypes.GetByIdAsync(request.BusinessTypeId, cancellationToken)
            ?? throw new NotFoundException(nameof(BusinessType), request.BusinessTypeId);

        if (bt.RoleTemplates.Count > 0)
            throw new DomainException("Não é possível excluir um tipo de negócio que possui templates de cargo. Remova os templates primeiro.");

        var counts = await _tenants.CountByBusinessTypeAsync(cancellationToken);
        if (counts.GetValueOrDefault(bt.Id, 0) > 0)
            throw new DomainException("Não é possível excluir um tipo de negócio que está em uso por tenants.");

        await _businessTypes.DeleteAsync(bt, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
