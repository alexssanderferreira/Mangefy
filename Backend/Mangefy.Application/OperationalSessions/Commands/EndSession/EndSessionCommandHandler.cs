using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.OperationalSessions.Repositories;
using MediatR;

namespace Mangefy.Application.OperationalSessions.Commands.EndSession;

public sealed class EndSessionCommandHandler : IRequestHandler<EndSessionCommand>
{
    private readonly IOperationalSessionRepository _sessions;
    private readonly IUnitOfWork _uow;

    public EndSessionCommandHandler(IOperationalSessionRepository sessions, IUnitOfWork uow)
    {
        _sessions = sessions;
        _uow = uow;
    }

    public async Task Handle(EndSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessions.GetByIdAsync(request.SessionId, cancellationToken)
            ?? throw new NotFoundException("OperationalSession", request.SessionId);

        if (session.TenantId != request.TenantId)
            throw new ForbiddenException();

        session.End();
        await _sessions.UpdateAsync(session, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
