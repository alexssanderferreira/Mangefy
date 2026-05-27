using Mangefy.Application.Platform.PlatformSuppliers.Queries.GetPlatformSuppliers;
using Mangefy.Domain.Common;
using Mangefy.Domain.Platform.Suppliers.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.PlatformSuppliers.Queries.GetPlatformSupplierById;

public sealed class GetPlatformSupplierByIdQueryHandler : IRequestHandler<GetPlatformSupplierByIdQuery, PlatformSupplierDto>
{
    private readonly IPlatformSupplierRepository _suppliers;

    public GetPlatformSupplierByIdQueryHandler(IPlatformSupplierRepository suppliers) => _suppliers = suppliers;

    public async Task<PlatformSupplierDto> Handle(GetPlatformSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var s = await _suppliers.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("Fornecedor não encontrado.");

        return new PlatformSupplierDto(
            s.Id, s.Name, s.Cnpj, s.SupplierCategoryId,
            s.Website, s.Email?.Value, s.Phone?.Value, s.Description, s.IsActive,
            s.Address?.Cep, s.Address?.Logradouro, s.Address?.Numero, s.Address?.Complemento,
            s.Address?.Bairro, s.Address?.Cidade, s.Address?.Uf,
            s.BusinessHours);
    }
}
