using MediatR;

namespace Mangefy.Application.Employees.Commands.GrantTemporaryAccess;

public sealed record GrantTemporaryAccessCommand(
    Guid TenantId,
    Guid EmployeeId,
    int ExtensionMinutes
) : IRequest;
