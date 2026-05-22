using MediatR;

namespace Mangefy.Application.Auth.Commands.SetPassword;

public sealed record SetPasswordCommand(
    string Token,
    string NewPassword
) : IRequest;
