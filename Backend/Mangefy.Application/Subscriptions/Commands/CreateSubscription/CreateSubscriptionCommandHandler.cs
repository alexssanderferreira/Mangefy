using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Plans;
using Mangefy.Domain.Plans.Repositories;
using Mangefy.Domain.Platform.Subscriptions;
using Mangefy.Domain.Platform.Subscriptions.Repositories;
using Mangefy.Domain.Tenants;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Subscriptions.Commands.CreateSubscription;

public sealed class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, Guid>
{
    private readonly ISubscriptionRepository _subscriptions;
    private readonly ITenantRepository _tenants;
    private readonly IPlanRepository _plans;
    private readonly IUnitOfWork _uow;

    public CreateSubscriptionCommandHandler(
        ISubscriptionRepository subscriptions,
        ITenantRepository tenants,
        IPlanRepository plans,
        IUnitOfWork uow)
    {
        _subscriptions = subscriptions;
        _tenants = tenants;
        _plans = plans;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        _ = await _tenants.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), request.TenantId);

        _ = await _plans.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(Plan), request.PlanId);

        var existing = await _subscriptions.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (existing is not null)
            throw new ConflictException("Tenant já possui uma assinatura ativa.");

        var subscription = Subscription.Create(request.TenantId, request.PlanId, request.StartDate, request.NextDueDate);
        await _subscriptions.AddAsync(subscription, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return subscription.Id;
    }
}
