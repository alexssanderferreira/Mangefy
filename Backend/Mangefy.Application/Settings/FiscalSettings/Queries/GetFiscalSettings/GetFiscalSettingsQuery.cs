using MediatR;

namespace Mangefy.Application.Settings.FiscalSettings.Queries.GetFiscalSettings;

public sealed record FiscalSettingsDto(
    Guid Id,
    bool NfceEnabled,
    bool AutoEmitOnTabClose,
    string? Cnpj);

public sealed record GetFiscalSettingsQuery(Guid TenantId)
    : IRequest<FiscalSettingsDto?>;
