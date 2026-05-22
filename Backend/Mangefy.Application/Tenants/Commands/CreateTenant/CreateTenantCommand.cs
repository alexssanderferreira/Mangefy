using MediatR;

namespace Mangefy.Application.Tenants.Commands.CreateTenant;

public sealed record CreateTenantCommand(
    Guid OwnerId,
    string Name,
    string Slug,
    Guid PlanId,
    Guid BusinessTypeId,
    string Timezone = "America/Sao_Paulo",
    int TrialDays = 14,
    string? Email = null
) : IRequest<Guid>;
