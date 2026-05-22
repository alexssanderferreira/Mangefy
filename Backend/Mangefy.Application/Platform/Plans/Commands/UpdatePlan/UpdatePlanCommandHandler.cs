using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Plans;
using Mangefy.Domain.Plans.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.Plans.Commands.UpdatePlan;

public sealed class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand>
{
    private readonly IPlanRepository _plans;
    private readonly IUnitOfWork _uow;

    public UpdatePlanCommandHandler(IPlanRepository plans, IUnitOfWork uow)
    {
        _plans = plans;
        _uow = uow;
    }

    public async Task Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _plans.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(Plan), request.PlanId);

        plan.UpdatePricing(request.MonthlyPrice);
        plan.UpdateLimits(request.MaxTables, request.MaxMenuItems, request.MaxUsers, request.MaxCustomRoles);
        plan.UpdateDescription(request.Description);

        await _plans.UpdateAsync(plan, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
