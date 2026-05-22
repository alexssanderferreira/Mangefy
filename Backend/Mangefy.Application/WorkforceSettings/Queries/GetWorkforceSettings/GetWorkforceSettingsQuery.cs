using MediatR;

namespace Mangefy.Application.WorkforceSettings.Queries.GetWorkforceSettings;

public sealed record WorkforceSettingsDto(Guid Id, int ShiftToleranceMinutes);

public sealed record GetWorkforceSettingsQuery(Guid TenantId)
    : IRequest<WorkforceSettingsDto?>;
