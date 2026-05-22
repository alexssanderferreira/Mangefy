using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Owners;
using Mangefy.Domain.Owners.Repositories;
using MediatR;

namespace Mangefy.Application.Auth.Commands.ActivateOwnerAccount;

public sealed class ActivateOwnerAccountCommandHandler : IRequestHandler<ActivateOwnerAccountCommand>
{
    private readonly IOwnerActivationTokenRepository _tokens;
    private readonly IOwnerRepository _owners;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _uow;

    public ActivateOwnerAccountCommandHandler(
        IOwnerActivationTokenRepository tokens,
        IOwnerRepository owners,
        IPasswordHasher passwordHasher,
        IUnitOfWork uow)
    {
        _tokens = tokens;
        _owners = owners;
        _passwordHasher = passwordHasher;
        _uow = uow;
    }

    public async Task Handle(ActivateOwnerAccountCommand request, CancellationToken cancellationToken)
    {
        var token = await _tokens.GetByTokenAsync(request.Token, cancellationToken)
            ?? throw new ForbiddenException("Token inválido ou expirado.");

        if (!token.IsValid())
            throw new ForbiddenException("Token inválido ou expirado.");

        var owner = await _owners.GetByIdAsync(token.OwnerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Owner), token.OwnerId);

        if (owner.Status == OwnerStatus.Inactive)
            throw new ForbiddenException("Conta de dono inativa. Contate o suporte.");

        owner.SetPassword(_passwordHasher.Hash(request.NewPassword));

        if (owner.Status == OwnerStatus.PendingActivation)
            owner.Activate();

        token.MarkAsUsed();

        await _owners.UpdateAsync(owner, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
