using MediatR;

namespace Mangefy.Application.Tenants.Commands.SuspendTenant;

public sealed record SuspendTenantCommand(Guid TenantId) : IRequest;
