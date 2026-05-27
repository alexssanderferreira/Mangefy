using MediatR;

namespace Mangefy.Application.Platform.Subscriptions.Queries.ListSubscriptions;

public sealed record ListSubscriptionsQuery : IRequest<IReadOnlyList<SubscriptionDto>>;

public sealed record SubscriptionDto(
    Guid Id,
    Guid TenantId,
    string TenantName,
    string TenantSlug,
    Guid PlanId,
    string PlanName,
    DateOnly StartDate,
    DateOnly NextDueDate,
    string? LatestInvoiceStatus,
    decimal? LatestInvoiceAmount,
    DateOnly? LatestInvoiceDueDate,
    int OverdueCount,
    IReadOnlyList<InvoiceDto> Invoices,
    string Status);

// Status possíveis: SemFaturas | EmDia | AguardandoPagamento | Inadimplente

public sealed record InvoiceDto(
    Guid Id,
    decimal Amount,
    string Currency,
    DateOnly DueDate,
    DateOnly? PaidAt,
    string Status,
    string? PaymentReference,
    string? Notes);
