using MediatR;

namespace Mangefy.Application.Auth.Commands.ResolveTenants;

public sealed record ResolveTenantsCommand(string Email, string Password) : IRequest<IReadOnlyList<TenantOptionDto>>;

public sealed record TenantOptionDto(Guid TenantId, string TenantSlug, string TenantName, string? LogoUrl);
