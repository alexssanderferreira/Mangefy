using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.BusinessTypes;
using Mangefy.Domain.Platform.BusinessTypes.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Commands.ToggleBusinessType;

public sealed class ToggleBusinessTypeCommandHandler : IRequestHandler<ToggleBusinessTypeCommand>
{
    private readonly IBusinessTypeRepository _businessTypes;
    private readonly IUnitOfWork _uow;

    public ToggleBusinessTypeCommandHandler(IBusinessTypeRepository businessTypes, IUnitOfWork uow)
    {
        _businessTypes = businessTypes;
        _uow = uow;
    }

    public async Task Handle(ToggleBusinessTypeCommand request, CancellationToken cancellationToken)
    {
        var bt = await _businessTypes.GetByIdAsync(request.BusinessTypeId, cancellationToken)
            ?? throw new NotFoundException(nameof(BusinessType), request.BusinessTypeId);

        if (request.Activate) bt.Activate();
        else bt.Deactivate();

        await _businessTypes.UpdateAsync(bt, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
