using MediatR;

namespace Mangefy.Application.Auth.Commands.SwitchTenant;

public sealed record SwitchTenantCommand(string TargetTenantSlug) : IRequest<SwitchTenantResult>;

public sealed record SwitchTenantResult(
    string AccessToken,
    DateTime ExpiresAt,
    Guid? EmployeeId,
    Guid? OwnerId,
    Guid TenantId,
    string Name,
    bool IsOwner,
    IReadOnlyList<string> Permissions);
