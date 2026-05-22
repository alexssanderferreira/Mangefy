using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Plans;
using Mangefy.Domain.Plans.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.Plans.Commands.CreatePlan;

public sealed class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, Guid>
{
    private readonly IPlanRepository _plans;
    private readonly IUnitOfWork _uow;

    public CreatePlanCommandHandler(IPlanRepository plans, IUnitOfWork uow)
    {
        _plans = plans;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = Plan.Create(
            request.Name, request.MonthlyPrice,
            request.MaxTables, request.MaxMenuItems, request.MaxUsers,
            request.MaxCustomRoles, request.Description);

        await _plans.AddAsync(plan, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return plan.Id;
    }
}
