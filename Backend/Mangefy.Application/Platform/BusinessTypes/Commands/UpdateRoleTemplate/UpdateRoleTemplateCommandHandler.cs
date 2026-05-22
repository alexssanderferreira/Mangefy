using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.BusinessTypes;
using Mangefy.Domain.Platform.BusinessTypes.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Commands.UpdateRoleTemplate;

public sealed class UpdateRoleTemplateCommandHandler : IRequestHandler<UpdateRoleTemplateCommand>
{
    private readonly IBusinessTypeRepository _businessTypes;
    private readonly IUnitOfWork _uow;

    public UpdateRoleTemplateCommandHandler(IBusinessTypeRepository businessTypes, IUnitOfWork uow)
    {
        _businessTypes = businessTypes;
        _uow = uow;
    }

    public async Task Handle(UpdateRoleTemplateCommand request, CancellationToken cancellationToken)
    {
        var bt = await _businessTypes.GetByIdAsync(request.BusinessTypeId, cancellationToken)
            ?? throw new NotFoundException(nameof(BusinessType), request.BusinessTypeId);

        bt.UpdateRoleTemplate(request.TemplateId, request.Name, request.Description);
        bt.SetRoleTemplatePermissions(request.TemplateId, request.Permissions);

        await _businessTypes.UpdateAsync(bt, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
