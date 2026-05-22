using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Common;
using Mangefy.Domain.Owners;
using Mangefy.Domain.Owners.Repositories;
using MediatR;

namespace Mangefy.Application.Owners.Commands.UpdateOwner;

public sealed class UpdateOwnerCommandHandler : IRequestHandler<UpdateOwnerCommand>
{
    private readonly IOwnerRepository _owners;
    private readonly IUnitOfWork _uow;

    public UpdateOwnerCommandHandler(IOwnerRepository owners, IUnitOfWork uow)
    {
        _owners = owners;
        _uow = uow;
    }

    public async Task Handle(UpdateOwnerCommand request, CancellationToken cancellationToken)
    {
        var owner = await _owners.GetByIdAsync(request.OwnerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Owner), request.OwnerId);

        owner.UpdateName(request.Name);

        // Atualiza e-mail apenas se mudou — valida unicidade entre owners
        if (!owner.Email.Value.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _owners.ExistsByEmailAsync(request.Email, cancellationToken))
                throw new ConflictException($"Já existe um cliente com o e-mail '{request.Email}'.");
            owner.ChangeEmail(request.Email);
        }

        OwnerDocumentType? docType = request.DocumentType?.ToUpperInvariant() switch
        {
            "CPF" => OwnerDocumentType.CPF,
            "CNPJ" => OwnerDocumentType.CNPJ,
            null => null,
            "" => null,
            _ => throw new DomainException("DocumentType inválido. Use CPF ou CNPJ.")
        };

        owner.UpdateContactInfo(request.Phone, docType, request.DocumentNumber, request.Notes);

        if (request.Address is null)
        {
            owner.SetAddress(null, null, null, null, null, null);
        }
        else
        {
            var a = request.Address;
            owner.SetAddress(a.Cep, a.Logradouro, a.Numero, a.Bairro, a.Cidade, a.Uf, a.Complemento);
        }

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
