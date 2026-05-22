using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Audit;
using Mangefy.Domain.Fiscal;
using Mangefy.Domain.Fiscal.Repositories;
using MediatR;

namespace Mangefy.Application.Fiscal.Commands.CancelFiscalDocument;

public sealed class CancelFiscalDocumentCommandHandler : IRequestHandler<CancelFiscalDocumentCommand>
{
    private readonly IFiscalDocumentRepository _fiscalDocs;
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _currentUser;

    public CancelFiscalDocumentCommandHandler(
        IFiscalDocumentRepository fiscalDocs,
        IUnitOfWork uow,
        IAuditService audit,
        ICurrentUser currentUser)
    {
        _fiscalDocs = fiscalDocs;
        _uow = uow;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(CancelFiscalDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = await _fiscalDocs.GetByIdAsync(request.DocumentId, cancellationToken)
            ?? throw new NotFoundException(nameof(FiscalDocument), request.DocumentId);

        if (doc.TenantId != request.TenantId)
            throw new ForbiddenException();

        doc.Cancel(request.Reason);

        await _fiscalDocs.UpdateAsync(doc, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            request.TenantId,
            _currentUser.EmployeeId,
            _currentUser.IsAdminSaas,
            AuditAction.FiscalDocumentCancelled,
            nameof(FiscalDocument),
            doc.Id,
            reason: request.Reason,
            ct: cancellationToken);
    }
}
