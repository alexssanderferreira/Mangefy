using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Common;
using Mangefy.Domain.Platform.Subscriptions.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.Subscriptions.Commands.GenerateInvoice;

public sealed class GenerateInvoiceCommandHandler : IRequestHandler<GenerateInvoiceCommand, Guid>
{
    private readonly ISubscriptionRepository _subscriptions;
    private readonly IUnitOfWork _uow;

    public GenerateInvoiceCommandHandler(ISubscriptionRepository subscriptions, IUnitOfWork uow)
    {
        _subscriptions = subscriptions;
        _uow = uow;
    }

    public async Task<Guid> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptions.GetByIdAsync(request.SubscriptionId, cancellationToken)
            ?? throw new DomainException("Assinatura não encontrada.");

        var invoice = subscription.GenerateInvoice(request.Amount, request.DueDate);
        await _subscriptions.UpdateAsync(subscription, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return invoice.Id;
    }
}
