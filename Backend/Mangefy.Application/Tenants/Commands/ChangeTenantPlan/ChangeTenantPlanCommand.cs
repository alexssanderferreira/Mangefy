using MediatR;

namespace Mangefy.Application.Tenants.Commands.ChangeTenantPlan;

public sealed record ChangeTenantPlanCommand(Guid TenantId, Guid NewPlanId) : IRequest;
