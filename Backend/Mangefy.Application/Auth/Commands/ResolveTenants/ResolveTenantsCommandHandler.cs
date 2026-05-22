using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Owners.Repositories;
using Mangefy.Domain.Tenants;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Auth.Commands.ResolveTenants;

public sealed class ResolveTenantsCommandHandler : IRequestHandler<ResolveTenantsCommand, IReadOnlyList<TenantOptionDto>>
{
    private readonly IOwnerRepository _owners;
    private readonly ITenantRepository _tenants;
    private readonly IPasswordHasher _passwordHasher;

    public ResolveTenantsCommandHandler(
        IOwnerRepository owners,
        ITenantRepository tenants,
        IPasswordHasher passwordHasher)
    {
        _owners = owners;
        _tenants = tenants;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<TenantOptionDto>> Handle(ResolveTenantsCommand request, CancellationToken cancellationToken)
    {
        var owner = await _owners.GetByEmailAsync(request.Email, cancellationToken);

        if (owner is null || owner.PasswordHash is null ||
            !_passwordHasher.Verify(request.Password, owner.PasswordHash))
            throw new ForbiddenException("Credenciais inválidas.");

        var tenants = await _tenants.GetByOwnerAsync(owner.Id, cancellationToken);

        var result = tenants
            .Where(t => t.Status != TenantStatus.Cancelled)
            .Select(t => new TenantOptionDto(t.Id, t.Slug, t.Name, t.LogoUrl))
            .ToList();

        if (result.Count == 0)
            throw new ForbiddenException("Nenhum estabelecimento ativo encontrado para este dono.");

        return result;
    }
}
