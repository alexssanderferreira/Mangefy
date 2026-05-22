using MediatR;

namespace Mangefy.Application.Settings.PaymentSettings.Queries.GetPaymentSettings;

public sealed record PaymentSettingsDto(Guid Id, IReadOnlyList<string> EnabledMethods);

public sealed record GetPaymentSettingsQuery(Guid TenantId)
    : IRequest<PaymentSettingsDto?>;
