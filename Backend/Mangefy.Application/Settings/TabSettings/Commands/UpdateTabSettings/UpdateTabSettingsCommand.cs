using MediatR;

namespace Mangefy.Application.Settings.TabSettings.Commands.UpdateTabSettings;

public sealed record UpdateTabSettingsCommand(
    Guid TenantId,
    int MinTabNumber,
    int MaxTabNumber,
    decimal MaxDiscountPercent = 10m,
    decimal? DiscountReasonRequiredAbove = null
) : IRequest;
