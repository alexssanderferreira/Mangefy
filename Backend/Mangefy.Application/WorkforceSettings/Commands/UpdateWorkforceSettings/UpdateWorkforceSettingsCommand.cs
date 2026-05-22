using MediatR;

namespace Mangefy.Application.WorkforceSettings.Commands.UpdateWorkforceSettings;

public sealed record UpdateWorkforceSettingsCommand(
    Guid TenantId,
    int ShiftToleranceMinutes
) : IRequest;
