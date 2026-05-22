using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Common;
using Mangefy.Domain.Owners;
using Mangefy.Domain.Owners.Repositories;
using MediatR;

namespace Mangefy.Application.Owners.Commands.ResendActivation;

public sealed class ResendActivationCommandHandler : IRequestHandler<ResendActivationCommand, ResendActivationResult>
{
    private readonly IOwnerRepository _owners;
    private readonly IOwnerActivationTokenRepository _tokens;
    private readonly IEmailSender _email;
    private readonly IUnitOfWork _uow;

    public ResendActivationCommandHandler(
        IOwnerRepository owners,
        IOwnerActivationTokenRepository tokens,
        IEmailSender email,
        IUnitOfWork uow)
    {
        _owners = owners;
        _tokens = tokens;
        _email = email;
        _uow = uow;
    }

    public async Task<ResendActivationResult> Handle(ResendActivationCommand request, CancellationToken cancellationToken)
    {
        var owner = await _owners.GetByIdAsync(request.OwnerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Owner), request.OwnerId);

        if (owner.Status == OwnerStatus.Active)
            throw new DomainException("Cliente já está ativo. Use 'Esqueci minha senha' caso queira redefinir.");

        if (owner.Status == OwnerStatus.Inactive)
            throw new DomainException("Cliente inativo. Reative antes de reenviar o e-mail.");

        var token = OwnerActivationToken.Create(owner.Id, TimeSpan.FromHours(48));
        await _tokens.AddAsync(token, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var emailSent = await _email.SendOwnerActivationAsync(owner.Email.Value, owner.Name, token.Token, cancellationToken);

        return new ResendActivationResult(token.Token, token.ExpiresAt, emailSent);
    }
}
