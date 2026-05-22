using Mangefy.Domain.Settings.Repositories;
using MediatR;

namespace Mangefy.Application.Settings.PaymentSettings.Queries.GetPaymentSettings;

public sealed class GetPaymentSettingsQueryHandler
    : IRequestHandler<GetPaymentSettingsQuery, PaymentSettingsDto?>
{
    private readonly IPaymentSettingsRepository _repository;

    public GetPaymentSettingsQueryHandler(IPaymentSettingsRepository repository)
        => _repository = repository;

    public async Task<PaymentSettingsDto?> Handle(
        GetPaymentSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (settings is null) return null;
        return new PaymentSettingsDto(settings.Id, settings.EnabledMethods.Select(m => m.ToString()).ToList());
    }
}
