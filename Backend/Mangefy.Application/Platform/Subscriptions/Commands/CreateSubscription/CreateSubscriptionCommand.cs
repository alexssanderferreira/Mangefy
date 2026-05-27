using MediatR;

namespace Mangefy.Application.Platform.Subscriptions.Commands.CreateSubscription;

public sealed record CreateSubscriptionCommand(
    Guid TenantId,
    Guid PlanId,
    DateOnly StartDate,
    DateOnly NextDueDate) : IRequest<Guid>;
