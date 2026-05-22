using MediatR;

namespace Mangefy.Application.Stock.Commands.RegisterPurchase;

public sealed record RegisterPurchaseCommand(
    Guid TenantId,
    Guid StockItemId,
    decimal Quantity,
    string? Reason,
    Guid EmployeeId
) : IRequest;
