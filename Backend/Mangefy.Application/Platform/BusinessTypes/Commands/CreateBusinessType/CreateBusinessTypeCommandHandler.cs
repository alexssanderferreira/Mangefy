using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.BusinessTypes;
using Mangefy.Domain.Platform.BusinessTypes.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Commands.CreateBusinessType;

public sealed class CreateBusinessTypeCommandHandler : IRequestHandler<CreateBusinessTypeCommand, Guid>
{
    private readonly IBusinessTypeRepository _businessTypes;
    private readonly IUnitOfWork _uow;

    public CreateBusinessTypeCommandHandler(IBusinessTypeRepository businessTypes, IUnitOfWork uow)
    {
        _businessTypes = businessTypes;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateBusinessTypeCommand request, CancellationToken cancellationToken)
    {
        if (await _businessTypes.ExistsByNameAsync(request.Name, cancellationToken))
            throw new ConflictException($"Já existe um tipo de negócio com o nome '{request.Name}'.");

        var businessType = BusinessType.Create(request.Name, request.Description);
        await _businessTypes.AddAsync(businessType, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return businessType.Id;
    }
}
