namespace Mangefy.Domain.Fiscal;

public enum FiscalDocumentStatus
{
    Pending,      // aguardando emissão
    Issued,       // emitido com sucesso
    Rejected,     // rejeitado pelo SEFAZ
    Cancelled,    // cancelado após emissão
    Contingency   // emitido em contingência (offline)
}
