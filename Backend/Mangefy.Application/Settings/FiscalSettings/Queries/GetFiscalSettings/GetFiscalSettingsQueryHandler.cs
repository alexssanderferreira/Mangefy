using Mangefy.Domain.Settings.Repositories;
using MediatR;

namespace Mangefy.Application.Settings.FiscalSettings.Queries.GetFiscalSettings;

public sealed class GetFiscalSettingsQueryHandler
    : IRequestHandler<GetFiscalSettingsQuery, FiscalSettingsDto?>
{
    private readonly IFiscalSettingsRepository _repository;

    public GetFiscalSettingsQueryHandler(IFiscalSettingsRepository repository)
        => _repository = repository;

    public async Task<FiscalSettingsDto?> Handle(
        GetFiscalSettingsQuery request, CancellationToken cancellationToken)
    {
        var s = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (s is null) return null;
        return new FiscalSettingsDto(s.Id, s.NfceEnabled, s.AutoEmitOnTabClose, s.Cnpj);
    }
}
