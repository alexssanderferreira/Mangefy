using MediatR;

namespace Mangefy.Application.Settings.FiscalSettings.Commands.UpdateFiscalSettings;

public sealed record UpdateFiscalSettingsCommand(
    Guid TenantId,
    bool NfceEnabled,
    string? Cnpj,
    string? FiscalHubApiKey,
    bool AutoEmitOnTabClose) : IRequest;
