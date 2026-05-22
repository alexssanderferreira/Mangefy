using MediatR;

namespace Mangefy.Application.Platform.Plans.Commands.DeactivatePlan;

public sealed record DeactivatePlanCommand(Guid PlanId) : IRequest;
