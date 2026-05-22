using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.OperationalSessions;
using Mangefy.Domain.OperationalSessions.Repositories;
using MediatR;

namespace Mangefy.Application.OperationalSessions.Commands.StartSession;

public sealed class StartSessionCommandHandler : IRequestHandler<StartSessionCommand, StartSessionResult>
{
    private readonly IOperationalSessionRepository _sessions;
    private readonly IShiftEnforcementService _shiftEnforcement;
    private readonly IUnitOfWork _uow;

    public StartSessionCommandHandler(
        IOperationalSessionRepository sessions,
        IShiftEnforcementService shiftEnforcement,
        IUnitOfWork uow)
    {
        _sessions = sessions;
        _shiftEnforcement = shiftEnforcement;
        _uow = uow;
    }

    public async Task<StartSessionResult> Handle(StartSessionCommand request, CancellationToken cancellationToken)
    {
        var isWithinShift = await _shiftEnforcement.CanOperateAsync(request.TenantId, request.EmployeeId, cancellationToken);

        // Encerrar sessão ativa anterior do mesmo funcionário
        var existing = await _sessions.GetActiveByEmployeeAsync(request.TenantId, request.EmployeeId, cancellationToken);
        if (existing is not null)
        {
            existing.End();
            await _sessions.UpdateAsync(existing, cancellationToken);
        }

        var session = OperationalSession.Start(
            request.TenantId,
            request.EmployeeId,
            isWithinShift,
            hasTemporaryAccess: false,
            deviceId: request.DeviceId);

        await _sessions.AddAsync(session, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new StartSessionResult(session.Id, isWithinShift);
    }
}
