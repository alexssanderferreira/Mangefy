using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.Features;
using Mangefy.Domain.Platform.Features.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.PlanFeatureSets.Commands.UpsertPlanFeatureSet;

public sealed class UpsertPlanFeatureSetCommandHandler : IRequestHandler<UpsertPlanFeatureSetCommand, Guid>
{
    private readonly IPlanFeatureSetRepository _featureSets;
    private readonly IUnitOfWork _uow;

    public UpsertPlanFeatureSetCommandHandler(IPlanFeatureSetRepository featureSets, IUnitOfWork uow)
    {
        _featureSets = featureSets;
        _uow = uow;
    }

    public async Task<Guid> Handle(UpsertPlanFeatureSetCommand request, CancellationToken cancellationToken)
    {
        var existing = await _featureSets.GetByPlanAndBusinessTypeAsync(request.PlanId, request.BusinessTypeId, cancellationToken);

        if (existing is null)
        {
            var fs = PlanFeatureSet.Create(request.PlanId, request.BusinessTypeId);
            foreach (var f in request.EnabledFeatures)
                fs.AddFeature(f);
            await _featureSets.AddAsync(fs, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            return fs.Id;
        }

        var currentFeatures = existing.EnabledFeatures.ToList();

        foreach (var f in request.EnabledFeatures.Except(currentFeatures))
            existing.AddFeature(f);

        foreach (var f in currentFeatures.Except(request.EnabledFeatures))
            existing.RemoveFeature(f);

        await _featureSets.UpdateAsync(existing, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return existing.Id;
    }
}
