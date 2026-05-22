using Mangefy.Domain.Settings.Repositories;
using MediatR;

namespace Mangefy.Application.Settings.TabSettings.Queries.GetTabSettings;

public sealed class GetTabSettingsQueryHandler
    : IRequestHandler<GetTabSettingsQuery, TabSettingsDto?>
{
    private readonly ITabSettingsRepository _repository;

    public GetTabSettingsQueryHandler(ITabSettingsRepository repository)
        => _repository = repository;

    public async Task<TabSettingsDto?> Handle(
        GetTabSettingsQuery request, CancellationToken cancellationToken)
    {
        var s = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (s is null) return null;
        return new TabSettingsDto(s.Id, s.MinTabNumber, s.MaxTabNumber, s.TotalNumbers, s.MaxDiscountPercent, s.DiscountReasonRequiredAbove);
    }
}
