using MediatR;

namespace Mangefy.Application.Fiscal.Commands.CancelFiscalDocument;

public sealed record CancelFiscalDocumentCommand(
    Guid TenantId,
    Guid DocumentId,
    string Reason
) : IRequest;
