using Mangefy.Domain.Fiscal;
using MediatR;

namespace Mangefy.Application.Fiscal.Queries.GetFiscalDocuments;

public sealed record GetFiscalDocumentsQuery(
    Guid TenantId,
    DateTime From,
    DateTime To
) : IRequest<IReadOnlyList<FiscalDocumentDto>>;

public sealed record FiscalDocumentDto(
    Guid Id,
    Guid TabId,
    FiscalDocumentType Type,
    FiscalDocumentStatus Status,
    FiscalEnvironment Environment,
    decimal TotalAmount,
    string? AccessKey,
    string? Protocol,
    string? RejectReason,
    string? CancellationReason,
    DateTime? IssuedAt,
    DateTime? CancelledAt,
    DateTime CreatedAt);
