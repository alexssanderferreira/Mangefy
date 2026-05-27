using Mangefy.API.Filters;
using Mangefy.Application.Platform.PlatformSuppliers.Commands.CreatePlatformSupplier;
using Mangefy.Application.Platform.PlatformSuppliers.Commands.DeletePlatformSupplier;
using Mangefy.Application.Platform.PlatformSuppliers.Commands.TogglePlatformSupplier;
using Mangefy.Application.Platform.PlatformSuppliers.Commands.UpdatePlatformSupplier;
using Mangefy.Application.Platform.PlatformSuppliers.Queries.GetPlatformSupplierById;
using Mangefy.Application.Platform.PlatformSuppliers.Queries.GetPlatformSuppliers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers.Admin;

[ApiController]
[Route("api/admin/platform-suppliers")]
[Authorize]
[RequireAdminSaas]
public sealed class PlatformSuppliersController : ControllerBase
{
    private readonly ISender _sender;

    public PlatformSuppliersController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId, CancellationToken ct)
        => Ok(await _sender.Send(new GetPlatformSuppliersQuery(categoryId), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new GetPlatformSupplierByIdQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlatformSupplierRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new CreatePlatformSupplierCommand(
            request.Name, request.SupplierCategoryId,
            request.Cnpj, request.Website, request.Email, request.Phone, request.Description,
            request.Cep, request.Logradouro, request.Numero, request.Bairro, request.Cidade, request.Uf, request.Complemento,
            request.BusinessHours), ct);
        return Created($"/api/admin/platform-suppliers/{id}", new { Id = id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlatformSupplierRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdatePlatformSupplierCommand(
            id, request.Name, request.SupplierCategoryId,
            request.Cnpj, request.Website, request.Email, request.Phone, request.Description,
            request.Cep, request.Logradouro, request.Numero, request.Bairro, request.Cidade, request.Uf, request.Complemento,
            request.BusinessHours), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new TogglePlatformSupplierCommand(id, Activate: true), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new TogglePlatformSupplierCommand(id, Activate: false), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeletePlatformSupplierCommand(id), ct);
        return NoContent();
    }
}

public sealed record CreatePlatformSupplierRequest(
    string Name,
    Guid SupplierCategoryId,
    string? Cnpj = null,
    string? Website = null,
    string? Email = null,
    string? Phone = null,
    string? Description = null,
    string? Cep = null,
    string? Logradouro = null,
    string? Numero = null,
    string? Bairro = null,
    string? Cidade = null,
    string? Uf = null,
    string? Complemento = null,
    string? BusinessHours = null);

public sealed record UpdatePlatformSupplierRequest(
    string Name,
    Guid SupplierCategoryId,
    string? Cnpj,
    string? Website,
    string? Email,
    string? Phone,
    string? Description,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Bairro,
    string? Cidade,
    string? Uf,
    string? Complemento,
    string? BusinessHours);
