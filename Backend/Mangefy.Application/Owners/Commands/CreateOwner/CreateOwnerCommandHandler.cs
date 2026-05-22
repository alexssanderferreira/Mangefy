using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Owners;
using Mangefy.Domain.Owners.Repositories;
using MediatR;

namespace Mangefy.Application.Owners.Commands.CreateOwner;

public sealed class CreateOwnerCommandHandler : IRequestHandler<CreateOwnerCommand, CreateOwnerResult>
{
    private readonly IOwnerRepository _owners;
    private readonly IOwnerActivationTokenRepository _tokens;
    private readonly IUnitOfWork _uow;
    private readonly IEmailSender _email;

    public CreateOwnerCommandHandler(
        IOwnerRepository owners,
        IOwnerActivationTokenRepository tokens,
        IUnitOfWork uow,
        IEmailSender email)
    {
        _owners = owners;
        _tokens = tokens;
        _uow = uow;
        _email = email;
    }

    public async Task<CreateOwnerResult> Handle(CreateOwnerCommand request, CancellationToken cancellationToken)
    {
        if (await _owners.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new ConflictException($"Já existe um owner com o e-mail '{request.Email}'.");

        var owner = Owner.Create(request.Name, request.Email);
        await _owners.AddAsync(owner, cancellationToken);

        var token = OwnerActivationToken.Create(owner.Id, TimeSpan.FromHours(48));
        await _tokens.AddAsync(token, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        // Envia e-mail de ativação (best-effort — falha não bloqueia a criação)
        await _email.SendOwnerActivationAsync(owner.Email.Value, owner.Name, token.Token, cancellationToken);

        return new CreateOwnerResult(owner.Id, token.Token, token.ExpiresAt);
    }
}
