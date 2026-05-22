using Mangefy.Domain.Fiscal.Repositories;
using MediatR;

namespace Mangefy.Application.Fiscal.Queries.GetFiscalDocuments;

public sealed class GetFiscalDocumentsQueryHandler : IRequestHandler<GetFiscalDocumentsQuery, IReadOnlyList<FiscalDocumentDto>>
{
    private readonly IFiscalDocumentRepository _fiscalDocs;

    public GetFiscalDocumentsQueryHandler(IFiscalDocumentRepository fiscalDocs)
    {
        _fiscalDocs = fiscalDocs;
    }

    public async Task<IReadOnlyList<FiscalDocumentDto>> Handle(GetFiscalDocumentsQuery request, CancellationToken cancellationToken)
    {
        var docs = await _fiscalDocs.GetByTenantAsync(request.TenantId, request.From, request.To, cancellationToken);

        return docs.Select(d => new FiscalDocumentDto(
            d.Id, d.TabId, d.Type, d.Status, d.Environment,
            d.TotalAmount.Amount, d.AccessKey, d.Protocol,
            d.RejectReason, d.CancellationReason,
            d.IssuedAt, d.CancelledAt, d.CreatedAt))
        .ToList();
    }
}
