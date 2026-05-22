using MediatR;

namespace Mangefy.Application.Roles.Commands.CreateRole;

public sealed record CreateRoleCommand(
    Guid TenantId,
    string Name,
    string? Description,
    IReadOnlyList<string> Permissions
) : IRequest<Guid>;
