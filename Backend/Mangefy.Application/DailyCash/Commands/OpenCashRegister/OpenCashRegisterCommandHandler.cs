using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.DailyCash;
using Mangefy.Domain.DailyCash.Repositories;
using Mangefy.Domain.Platform.Features;
using MediatR;

namespace Mangefy.Application.DailyCash.Commands.OpenCashRegister;

public sealed class OpenCashRegisterCommandHandler : IRequestHandler<OpenCashRegisterCommand, Guid>
{
    private readonly ICashRegisterRepository _cashRegisters;
    private readonly IFeatureGateService _featureGate;
    private readonly IUnitOfWork _uow;

    public OpenCashRegisterCommandHandler(
        ICashRegisterRepository cashRegisters,
        IFeatureGateService featureGate,
        IUnitOfWork uow)
    {
        _cashRegisters = cashRegisters;
        _featureGate = featureGate;
        _uow = uow;
    }

    public async Task<Guid> Handle(OpenCashRegisterCommand request, CancellationToken cancellationToken)
    {
        await _featureGate.RequireAsync(request.TenantId, FeatureCatalog.Cash.DailyClose, cancellationToken);
        var existing = await _cashRegisters.GetOpenByTenantAsync(request.TenantId, cancellationToken);
        if (existing is not null)
            throw new ConflictException("Já existe um caixa aberto para este estabelecimento.");

        var cashRegister = CashRegister.Open(request.TenantId, request.OpeningAmount, request.EmployeeId);
        await _cashRegisters.AddAsync(cashRegister, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return cashRegister.Id;
    }
}
