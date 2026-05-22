using MediatR;

namespace Mangefy.Application.Auth.Commands.Login;

public sealed record LoginCommand(string TenantSlug, string Email, string Password) : IRequest<LoginResult>;

public sealed record LoginResult(
    string AccessToken,
    DateTime ExpiresAt,
    Guid? EmployeeId,
    Guid? OwnerId,
    Guid TenantId,
    string Name,
    bool IsOwner,
    IReadOnlyList<string> Permissions
);
