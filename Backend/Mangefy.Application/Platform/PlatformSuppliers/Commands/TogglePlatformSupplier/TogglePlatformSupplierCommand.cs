using MediatR;

namespace Mangefy.Application.Platform.PlatformSuppliers.Commands.TogglePlatformSupplier;

public sealed record TogglePlatformSupplierCommand(Guid Id, bool Activate) : IRequest;
