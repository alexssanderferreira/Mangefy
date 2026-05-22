using MediatR;

namespace Mangefy.Application.Settings.PaymentSettings.Commands.UpdatePaymentSettings;

public sealed record UpdatePaymentSettingsCommand(
    Guid TenantId,
    IReadOnlyList<string> EnabledMethods) : IRequest;
