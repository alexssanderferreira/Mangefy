using Mangefy.API.Filters;
using Mangefy.Application.Stock.Commands.AddStockItem;
using Mangefy.Application.Stock.Commands.AdjustInventory;
using Mangefy.Application.Stock.Commands.RegisterPurchase;
using Mangefy.Application.Stock.Queries.GetStockByTenant;
using Mangefy.Domain.Roles;
using Mangefy.Domain.Stock;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/stock")]
[Authorize]
[ValidateTenantAccess]
public sealed class StockController : ControllerBase
{
    private readonly ISender _sender;
    public StockController(ISender sender) => _sender = sender;

    [HttpGet]
    [RequirePermission(PermissionCatalog.Stock.Read)]
    public async Task<IActionResult> GetAll(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetStockByTenantQuery(tenantId), ct));

    [HttpPost("items")]
    [RequirePermission(PermissionCatalog.Stock.Manage)]
    public async Task<IActionResult> AddItem(Guid tenantId, [FromBody] AddStockItemRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new AddStockItemCommand(
            tenantId, request.Name, request.Unit, request.CurrentQuantity,
            request.MinimumQuantity, request.CostPerUnit, request.Station, request.SupplierId), ct);
        return Created(string.Empty, new { id });
    }

    [HttpPatch("items/{stockItemId:guid}/adjust")]
    [RequirePermission(PermissionCatalog.Stock.Manage)]
    public async Task<IActionResult> AdjustInventory(Guid tenantId, Guid stockItemId, [FromBody] AdjustInventoryRequest request, CancellationToken ct)
    {
        await _sender.Send(new AdjustInventoryCommand(tenantId, stockItemId, request.NewQuantity, request.Reason), ct);
        return NoContent();
    }

    [HttpPost("items/{stockItemId:guid}/purchase")]
    [RequirePermission(PermissionCatalog.Stock.Manage)]
    public async Task<IActionResult> RegisterPurchase(Guid tenantId, Guid stockItemId, [FromBody] RegisterPurchaseRequest request, CancellationToken ct)
    {
        await _sender.Send(new RegisterPurchaseCommand(tenantId, stockItemId, request.Quantity, request.Reason, request.EmployeeId), ct);
        return NoContent();
    }
}

public sealed record AddStockItemRequest(string Name, StockUnit Unit, decimal CurrentQuantity, decimal MinimumQuantity, decimal CostPerUnit, StockStation Station, Guid? SupplierId);
public sealed record AdjustInventoryRequest(decimal NewQuantity, string Reason);
public sealed record RegisterPurchaseRequest(decimal Quantity, string? Reason, Guid EmployeeId);
