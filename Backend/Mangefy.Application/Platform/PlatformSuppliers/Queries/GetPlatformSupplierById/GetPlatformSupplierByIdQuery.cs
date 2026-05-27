using Mangefy.Application.Platform.PlatformSuppliers.Queries.GetPlatformSuppliers;
using MediatR;

namespace Mangefy.Application.Platform.PlatformSuppliers.Queries.GetPlatformSupplierById;

public sealed record GetPlatformSupplierByIdQuery(Guid Id) : IRequest<PlatformSupplierDto>;
