using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Plans;
using Mangefy.Domain.Plans.Repositories;
using Mangefy.Domain.Tables;
using Mangefy.Domain.Tables.Repositories;
using Mangefy.Domain.Tenants;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Tables.Commands.CreateTable;

public sealed class CreateTableCommandHandler : IRequestHandler<CreateTableCommand, Guid>
{
    private readonly ITableRepository _tables;
    private readonly ITenantRepository _tenants;
    private readonly IPlanRepository _plans;
    private readonly IUnitOfWork _uow;

    public CreateTableCommandHandler(
        ITableRepository tables,
        ITenantRepository tenants,
        IPlanRepository plans,
        IUnitOfWork uow)
    {
        _tables = tables;
        _tenants = tenants;
        _plans = plans;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateTableCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenants.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), request.TenantId);

        var plan = await _plans.GetByIdAsync(tenant.PlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(Plan), tenant.PlanId);

        var currentCount = await _tables.CountByTenantAsync(request.TenantId, cancellationToken);
        if (currentCount >= plan.MaxTables)
            throw new ConflictException($"Limite de {plan.MaxTables} mesa(s) atingido pelo plano atual.");

        if (await _tables.ExistsByNumberAsync(request.TenantId, request.Number, cancellationToken))
            throw new ConflictException($"Mesa '{request.Number}' já existe.");

        var table = Table.Create(request.TenantId, request.Number, request.Capacity, request.Section);
        await _tables.AddAsync(table, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return table.Id;
    }
}
