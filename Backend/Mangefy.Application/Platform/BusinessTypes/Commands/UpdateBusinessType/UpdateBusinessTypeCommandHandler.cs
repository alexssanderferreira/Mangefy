using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.BusinessTypes;
using Mangefy.Domain.Platform.BusinessTypes.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Commands.UpdateBusinessType;

public sealed class UpdateBusinessTypeCommandHandler : IRequestHandler<UpdateBusinessTypeCommand>
{
    private readonly IBusinessTypeRepository _businessTypes;
    private readonly IUnitOfWork _uow;

    public UpdateBusinessTypeCommandHandler(IBusinessTypeRepository businessTypes, IUnitOfWork uow)
    {
        _businessTypes = businessTypes;
        _uow = uow;
    }

    public async Task Handle(UpdateBusinessTypeCommand request, CancellationToken cancellationToken)
    {
        var bt = await _businessTypes.GetByIdAsync(request.BusinessTypeId, cancellationToken)
            ?? throw new NotFoundException(nameof(BusinessType), request.BusinessTypeId);

        bt.UpdateInfo(request.Name, request.Description);
        await _businessTypes.UpdateAsync(bt, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
