using MediatR;

namespace Mangefy.Application.Platform.PlanFeatureSets.Queries.GetPlanFeatureSets;

public sealed record GetPlanFeatureSetsQuery(Guid PlanId) : IRequest<IReadOnlyList<PlanFeatureSetDto>>;

public sealed record PlanFeatureSetDto(
    Guid Id,
    Guid PlanId,
    Guid BusinessTypeId,
    IReadOnlyCollection<string> EnabledFeatures);
