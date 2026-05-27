using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Common;
using Mangefy.Domain.Platform.Subscriptions;
using Mangefy.Domain.Platform.Subscriptions.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.Subscriptions.Commands.CreateSubscription;

public sealed class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, Guid>
{
    private readonly ISubscriptionRepository _subscriptions;
    private readonly IUnitOfWork _uow;

    public CreateSubscriptionCommandHandler(ISubscriptionRepository subscriptions, IUnitOfWork uow)
    {
        _subscriptions = subscriptions;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var existing = await _subscriptions.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (existing is not null)
            throw new DomainException("Tenant já possui assinatura.");

        var subscription = Subscription.Create(request.TenantId, request.PlanId, request.StartDate, request.NextDueDate);
        await _subscriptions.AddAsync(subscription, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return subscription.Id;
    }
}
