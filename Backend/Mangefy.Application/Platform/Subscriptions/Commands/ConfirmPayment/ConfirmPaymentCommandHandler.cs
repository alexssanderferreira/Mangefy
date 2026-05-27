using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Common;
using Mangefy.Domain.Platform.Subscriptions.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.Subscriptions.Commands.ConfirmPayment;

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
        var subscription = await _subscriptions.GetByIdAsync(request.SubscriptionId, cancellationToken)
            ?? throw new DomainException("Assinatura não encontrada.");

        subscription.ConfirmPayment(request.InvoiceId, request.PaidAt, request.NextDueDate,
            request.PaymentReference, request.Notes);

        await _subscriptions.UpdateAsync(subscription, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
