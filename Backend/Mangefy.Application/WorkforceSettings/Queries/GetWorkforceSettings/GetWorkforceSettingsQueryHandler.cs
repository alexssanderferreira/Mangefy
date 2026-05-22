using Mangefy.Domain.Settings.Repositories;
using MediatR;

namespace Mangefy.Application.WorkforceSettings.Queries.GetWorkforceSettings;

public sealed class GetWorkforceSettingsQueryHandler
    : IRequestHandler<GetWorkforceSettingsQuery, WorkforceSettingsDto?>
{
    private readonly IWorkforceSettingsRepository _repository;

    public GetWorkforceSettingsQueryHandler(IWorkforceSettingsRepository repository)
        => _repository = repository;

    public async Task<WorkforceSettingsDto?> Handle(
        GetWorkforceSettingsQuery request, CancellationToken cancellationToken)
    {
        var s = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (s is null) return null;
        return new WorkforceSettingsDto(s.Id, s.ShiftToleranceMinutes);
    }
}
