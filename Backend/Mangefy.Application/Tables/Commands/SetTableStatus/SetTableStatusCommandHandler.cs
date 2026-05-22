using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Tables.Repositories;
using MediatR;

namespace Mangefy.Application.Tables.Commands.SetTableStatus;

public sealed class SetTableStatusCommandHandler : IRequestHandler<SetTableStatusCommand>
{
    private readonly ITableRepository _tables;
    private readonly IUnitOfWork _uow;

    public SetTableStatusCommandHandler(ITableRepository tables, IUnitOfWork uow)
    {
        _tables = tables;
        _uow = uow;
    }

    public async Task Handle(SetTableStatusCommand request, CancellationToken cancellationToken)
    {
        var table = await _tables.GetByIdAsync(request.TableId, cancellationToken)
            ?? throw new NotFoundException("Mesa", request.TableId);

        if (table.TenantId != request.TenantId)
            throw new ForbiddenException();

        if (request.Status == "Unavailable")
            table.MarkAsUnavailable();
        else
            table.Release(); // volta para Available

        await _tables.UpdateAsync(table, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
