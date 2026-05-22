using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Plans;
using Mangefy.Domain.Plans.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.Plans.Commands.DeletePlan;

public sealed class DeletePlanCommandHandler : IRequestHandler<DeletePlanCommand>
{
    private readonly IPlanRepository _plans;
    private readonly IUnitOfWork _uow;

    public DeletePlanCommandHandler(IPlanRepository plans, IUnitOfWork uow)
    {
        _plans = plans;
        _uow = uow;
    }

    public async Task Handle(DeletePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _plans.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(Plan), request.PlanId);

        await _plans.DeleteAsync(plan, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
