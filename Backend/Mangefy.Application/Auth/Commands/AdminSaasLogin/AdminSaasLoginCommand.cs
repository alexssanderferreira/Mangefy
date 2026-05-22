using MediatR;

namespace Mangefy.Application.Auth.Commands.AdminSaasLogin;

public sealed record AdminSaasLoginCommand(string Email, string Password) : IRequest<AdminSaasLoginResult>;

public sealed record AdminSaasLoginResult(
    string AccessToken,
    DateTime ExpiresAt,
    string Email
);
