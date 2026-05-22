using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.Subscriptions.Repositories;
using MediatR;

namespace Mangefy.Application.Subscriptions.Commands.ConfirmPayment;

public sealed class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand>
{
    private readonly ISubscriptionRepository _subscriptions;
    private readonly IUnitOfWork _uow;

    public ConfirmPaymentCommandHandler(ISubscriptionRepository subscriptions, IUnitOfWork uow)
    {
        _subscriptions = subscriptions;
        _uow = uow;
    }

    public async Task Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptions.GetByTenantIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException("Subscription", request.TenantId);

        subscription.ConfirmPayment(
            request.InvoiceId, request.PaidAt, request.NextDueDate,
            request.PaymentReference, request.Notes);

        await _subscriptions.UpdateAsync(subscription, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
