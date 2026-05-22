using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Tables.Repositories;
using MediatR;

namespace Mangefy.Application.Tables.Commands.UpdateTable;

public sealed class UpdateTableCommandHandler : IRequestHandler<UpdateTableCommand>
{
    private readonly ITableRepository _tables;
    private readonly IUnitOfWork _uow;

    public UpdateTableCommandHandler(ITableRepository tables, IUnitOfWork uow)
    {
        _tables = tables;
        _uow = uow;
    }

    public async Task Handle(UpdateTableCommand request, CancellationToken cancellationToken)
    {
        var table = await _tables.GetByIdAsync(request.TableId, cancellationToken)
            ?? throw new NotFoundException("Mesa", request.TableId);

        if (table.TenantId != request.TenantId)
            throw new ForbiddenException();

        var numberConflict = await _tables.GetByNumberAsync(request.TenantId, request.Number, cancellationToken);
        if (numberConflict is not null && numberConflict.Id != request.TableId)
            throw new ConflictException($"Já existe uma mesa com o número '{request.Number}'.");

        table.UpdateInfo(request.Number, request.Capacity, request.Section);
        await _tables.UpdateAsync(table, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
