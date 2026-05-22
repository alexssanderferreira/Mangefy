using MediatR;

namespace Mangefy.Application.Auth.Commands.ActivateOwnerAccount;

public sealed record ActivateOwnerAccountCommand(
    string Token,
    string NewPassword
) : IRequest;
