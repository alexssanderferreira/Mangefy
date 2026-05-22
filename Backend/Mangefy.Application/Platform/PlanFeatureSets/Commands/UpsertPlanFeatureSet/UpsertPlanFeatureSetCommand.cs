using MediatR;

namespace Mangefy.Application.Platform.PlanFeatureSets.Commands.UpsertPlanFeatureSet;

public sealed record UpsertPlanFeatureSetCommand(
    Guid PlanId,
    Guid BusinessTypeId,
    IReadOnlyList<string> EnabledFeatures) : IRequest<Guid>;
