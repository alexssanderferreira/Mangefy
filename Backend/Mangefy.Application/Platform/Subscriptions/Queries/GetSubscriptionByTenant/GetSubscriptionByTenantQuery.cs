using Mangefy.Application.Platform.Subscriptions.Queries.ListSubscriptions;
using MediatR;

namespace Mangefy.Application.Platform.Subscriptions.Queries.GetSubscriptionByTenant;

public sealed record GetSubscriptionByTenantQuery(Guid TenantId) : IRequest<SubscriptionDto?>;
