using MediatR;

namespace Mangefy.Application.Tenants.Commands.ChangeBusinessType;

public sealed record ChangeBusinessTypeCommand(Guid TenantId, Guid BusinessTypeId) : IRequest;
