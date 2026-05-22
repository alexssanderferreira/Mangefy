using MediatR;

namespace Mangefy.Application.Platform.PlatformSuppliers.Commands.DeletePlatformSupplier;

public sealed record DeletePlatformSupplierCommand(Guid Id) : IRequest;
