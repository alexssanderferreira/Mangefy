using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Plans;
using Mangefy.Domain.Plans.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.Plans.Commands.DeactivatePlan;

public sealed class DeactivatePlanCommandHandler : IRequestHandler<DeactivatePlanCommand>
{
    private readonly IPlanRepository _plans;
    private readonly IUnitOfWork _uow;

    public DeactivatePlanCommandHandler(IPlanRepository plans, IUnitOfWork uow)
    {
        _plans = plans;
        _uow = uow;
    }

    public async Task Handle(DeactivatePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _plans.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(Plan), request.PlanId);

        plan.Deactivate();
        await _plans.UpdateAsync(plan, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
