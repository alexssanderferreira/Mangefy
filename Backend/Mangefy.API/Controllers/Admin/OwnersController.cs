using Mangefy.API.Filters;
using Mangefy.Application.Owners.Commands.ActivateOwner;
using Mangefy.Application.Owners.Commands.CreateOwner;
using Mangefy.Application.Owners.Commands.DeactivateOwner;
using Mangefy.Application.Owners.Commands.ResendActivation;
using Mangefy.Application.Owners.Commands.UpdateOwner;
using Mangefy.Application.Owners.Queries.GetOwnerById;
using Mangefy.Application.Owners.Queries.ListOwners;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers.Admin;

[ApiController]
[Route("api/admin/owners")]
[Authorize]
[RequireAdminSaas]
public sealed class OwnersController : ControllerBase
{
    private readonly ISender _sender;

    public OwnersController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await _sender.Send(new ListOwnersQuery(page, pageSize), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new GetOwnerByIdQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOwnerRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new CreateOwnerCommand(request.Name, request.Email), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOwnerRequest request, CancellationToken ct)
    {
        UpdateOwnerAddressDto? addr = request.Address is null ? null : new UpdateOwnerAddressDto(
            request.Address.Cep, request.Address.Logradouro, request.Address.Numero,
            request.Address.Complemento, request.Address.Bairro, request.Address.Cidade, request.Address.Uf);

        await _sender.Send(new UpdateOwnerCommand(
            id, request.Name, request.Email, request.Phone, request.DocumentType, request.DocumentNumber, request.Notes, addr), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new ActivateOwnerCommand(id), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeactivateOwnerCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/resend-activation")]
    public async Task<IActionResult> ResendActivation(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new ResendActivationCommand(id), ct));
}

public sealed record CreateOwnerRequest(string Name, string Email);

public sealed record UpdateOwnerRequest(
    string Name,
    string Email,
    string? Phone,
    string? DocumentType,
    string? DocumentNumber,
    string? Notes,
    UpdateOwnerAddressRequest? Address);

public sealed record UpdateOwnerAddressRequest(
    string Cep,
    string Logradouro,
    string Numero,
    string? Complemento,
    string Bairro,
    string Cidade,
    string Uf);
