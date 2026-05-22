using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.BusinessTypes;
using Mangefy.Domain.Platform.BusinessTypes.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Commands.ToggleRoleTemplate;

public sealed class ToggleRoleTemplateCommandHandler : IRequestHandler<ToggleRoleTemplateCommand>
{
    private readonly IBusinessTypeRepository _businessTypes;
    private readonly IUnitOfWork _uow;

    public ToggleRoleTemplateCommandHandler(IBusinessTypeRepository businessTypes, IUnitOfWork uow)
    {
        _businessTypes = businessTypes;
        _uow = uow;
    }

    public async Task Handle(ToggleRoleTemplateCommand request, CancellationToken cancellationToken)
    {
        var bt = await _businessTypes.GetByIdAsync(request.BusinessTypeId, cancellationToken)
            ?? throw new NotFoundException(nameof(BusinessType), request.BusinessTypeId);

        if (request.Activate)
            bt.ActivateRoleTemplate(request.TemplateId);
        else
            bt.DeactivateRoleTemplate(request.TemplateId);

        await _businessTypes.UpdateAsync(bt, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
