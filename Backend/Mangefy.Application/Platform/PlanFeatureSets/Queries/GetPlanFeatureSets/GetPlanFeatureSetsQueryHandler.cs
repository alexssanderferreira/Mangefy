using Mangefy.Domain.Platform.Features.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.PlanFeatureSets.Queries.GetPlanFeatureSets;

public sealed class GetPlanFeatureSetsQueryHandler : IRequestHandler<GetPlanFeatureSetsQuery, IReadOnlyList<PlanFeatureSetDto>>
{
    private readonly IPlanFeatureSetRepository _featureSets;

    public GetPlanFeatureSetsQueryHandler(IPlanFeatureSetRepository featureSets) => _featureSets = featureSets;

    public async Task<IReadOnlyList<PlanFeatureSetDto>> Handle(GetPlanFeatureSetsQuery request, CancellationToken cancellationToken)
    {
        var list = await _featureSets.GetByPlanAsync(request.PlanId, cancellationToken);
        return list.Select(fs => new PlanFeatureSetDto(fs.Id, fs.PlanId, fs.BusinessTypeId, fs.EnabledFeatures)).ToList();
    }
}
