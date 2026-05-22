using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Owners;
using Mangefy.Domain.Owners.Repositories;
using MediatR;

namespace Mangefy.Application.Owners.Commands.DeactivateOwner;

public sealed class DeactivateOwnerCommandHandler : IRequestHandler<DeactivateOwnerCommand>
{
    private readonly IOwnerRepository _owners;
    private readonly IUnitOfWork _uow;

    public DeactivateOwnerCommandHandler(IOwnerRepository owners, IUnitOfWork uow)
    {
        _owners = owners;
        _uow = uow;
    }

    public async Task Handle(DeactivateOwnerCommand request, CancellationToken cancellationToken)
    {
        var owner = await _owners.GetByIdAsync(request.OwnerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Owner), request.OwnerId);

        owner.Deactivate();
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
