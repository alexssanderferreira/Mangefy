using MediatR;

namespace Mangefy.Application.Settings.TabSettings.Queries.GetTabSettings;

public sealed record TabSettingsDto(
    Guid Id,
    int MinTabNumber,
    int MaxTabNumber,
    int TotalNumbers,
    decimal MaxDiscountPercent,
    decimal? DiscountReasonRequiredAbove);

public sealed record GetTabSettingsQuery(Guid TenantId)
    : IRequest<TabSettingsDto?>;
