using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Tables;
using Mangefy.Domain.Tables.Repositories;
using MediatR;

namespace Mangefy.Application.Tables.Commands.CreateTable;

public sealed class CreateTableCommandHandler : IRequestHandler<CreateTableCommand, Guid>
{
    private readonly ITableRepository _tables;
    private readonly IUnitOfWork _uow;

    public CreateTableCommandHandler(ITableRepository tables, IUnitOfWork uow)
    {
        _tables = tables;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateTableCommand request, CancellationToken cancellationToken)
    {
        if (await _tables.ExistsByNumberAsync(request.TenantId, request.Number, cancellationToken))
            throw new ConflictException($"Mesa '{request.Number}' já existe.");

        var table = Table.Create(request.TenantId, request.Number, request.Capacity, request.Section);
        await _tables.AddAsync(table, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return table.Id;
    }
}
