using MediatR;

namespace Mangefy.Application.Tenants.Commands.CancelTenant;

public sealed record CancelTenantCommand(Guid TenantId) : IRequest;
