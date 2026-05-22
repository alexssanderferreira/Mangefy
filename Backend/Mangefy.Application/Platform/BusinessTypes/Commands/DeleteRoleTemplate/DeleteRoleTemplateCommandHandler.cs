using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Common;
using Mangefy.Domain.Platform.BusinessTypes;
using Mangefy.Domain.Platform.BusinessTypes.Repositories;
using Mangefy.Domain.Roles.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Commands.DeleteRoleTemplate;

public sealed class DeleteRoleTemplateCommandHandler : IRequestHandler<DeleteRoleTemplateCommand>
{
    private readonly IBusinessTypeRepository _businessTypes;
    private readonly ITenantRoleRepository _tenantRoles;
    private readonly IUnitOfWork _uow;

    public DeleteRoleTemplateCommandHandler(
        IBusinessTypeRepository businessTypes,
        ITenantRoleRepository tenantRoles,
        IUnitOfWork uow)
    {
        _businessTypes = businessTypes;
        _tenantRoles = tenantRoles;
        _uow = uow;
    }

    public async Task Handle(DeleteRoleTemplateCommand request, CancellationToken cancellationToken)
    {
        var bt = await _businessTypes.GetByIdAsync(request.BusinessTypeId, cancellationToken)
            ?? throw new NotFoundException(nameof(BusinessType), request.BusinessTypeId);

        var counts = await _tenantRoles.CountByTemplateIdAsync(cancellationToken);
        if (counts.GetValueOrDefault(request.TemplateId, 0) > 0)
            throw new DomainException("Não é possível excluir um template em uso por tenants. Desative-o em vez disso.");

        bt.RemoveRoleTemplate(request.TemplateId);

        await _businessTypes.UpdateAsync(bt, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
