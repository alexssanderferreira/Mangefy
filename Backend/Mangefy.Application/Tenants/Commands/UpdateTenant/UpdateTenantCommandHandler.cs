using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Tenants;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Tenants.Commands.UpdateTenant;

public sealed class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand>
{
    private readonly ITenantRepository _tenants;
    private readonly IUnitOfWork _uow;

    public UpdateTenantCommandHandler(ITenantRepository tenants, IUnitOfWork uow)
    {
        _tenants = tenants;
        _uow = uow;
    }

    public async Task Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenants.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), request.TenantId);

        tenant.UpdateInfo(request.Name, request.LogoUrl, request.Email, request.Timezone);

        if (!string.IsNullOrWhiteSpace(request.Phone))
            tenant.SetPhone(request.Phone);

        if (!string.IsNullOrWhiteSpace(request.Cep) && !string.IsNullOrWhiteSpace(request.Logradouro) &&
            !string.IsNullOrWhiteSpace(request.Numero) && !string.IsNullOrWhiteSpace(request.Bairro) &&
            !string.IsNullOrWhiteSpace(request.Cidade) && !string.IsNullOrWhiteSpace(request.Uf))
            tenant.SetAddress(request.Cep, request.Logradouro, request.Numero,
                request.Bairro, request.Cidade, request.Uf, request.Complemento);
        await _tenants.UpdateAsync(tenant, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
