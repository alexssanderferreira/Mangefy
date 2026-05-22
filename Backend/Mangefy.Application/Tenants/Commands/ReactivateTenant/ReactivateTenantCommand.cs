using MediatR;

namespace Mangefy.Application.Tenants.Commands.ReactivateTenant;

public sealed record ReactivateTenantCommand(Guid TenantId) : IRequest;
