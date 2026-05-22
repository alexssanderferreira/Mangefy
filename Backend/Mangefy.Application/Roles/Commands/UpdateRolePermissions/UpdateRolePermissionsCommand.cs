using MediatR;

namespace Mangefy.Application.Roles.Commands.UpdateRolePermissions;

public sealed record UpdateRolePermissionsCommand(
    Guid TenantId,
    Guid RoleId,
    IReadOnlyList<string> Permissions
) : IRequest;
