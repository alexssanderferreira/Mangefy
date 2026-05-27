using Mangefy.Application.Platform.Subscriptions.Queries.ListSubscriptions;
using MediatR;

namespace Mangefy.Application.Platform.Subscriptions.Queries.GetOverdueSubscriptions;

public sealed record GetOverdueSubscriptionsQuery : IRequest<IReadOnlyList<SubscriptionDto>>;
