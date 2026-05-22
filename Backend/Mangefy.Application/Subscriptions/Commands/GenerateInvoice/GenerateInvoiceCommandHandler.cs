using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.Subscriptions;
using Mangefy.Domain.Platform.Subscriptions.Repositories;
using MediatR;

namespace Mangefy.Application.Subscriptions.Commands.GenerateInvoice;

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
        var subscription = await _subscriptions.GetByTenantIdAsync(request.TenantId, cancellationToken);

        if (subscription is null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            // Sem planId disponível aqui — caller deve criar Subscription no onboarding do tenant.
            // Lançar erro: assinatura deve existir antes de gerar fatura.
            throw new NotFoundException("Subscription", request.TenantId);
        }

        var invoice = subscription.GenerateInvoice(request.Amount, request.DueDate);
        await _subscriptions.UpdateAsync(subscription, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return invoice.Id;
    }
}
