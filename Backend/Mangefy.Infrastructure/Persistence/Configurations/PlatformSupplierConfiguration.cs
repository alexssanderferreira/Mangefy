using Mangefy.Domain.Platform.Suppliers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class PlatformSupplierConfiguration : IEntityTypeConfiguration<PlatformSupplier>
{
    public void Configure(EntityTypeBuilder<PlatformSupplier> builder)
    {
        builder.ToTable("PlatformSuppliers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Cnpj).HasMaxLength(20);
        builder.Property(x => x.SupplierCategoryId).IsRequired();
        builder.Property(x => x.Website).HasMaxLength(300);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.IsActive).IsRequired();

        builder.Property(x => x.BusinessHours).HasMaxLength(300);

        builder.OwnsOne(x => x.Email, e =>
            e.Property(x => x.Value).HasColumnName("Email").HasMaxLength(200));

        builder.OwnsOne(x => x.Phone, p =>
            p.Property(x => x.Value).HasColumnName("Phone").HasMaxLength(30));

        builder.OwnsOne(x => x.Address, a =>
        {
            a.Property(x => x.Cep).HasColumnName("AddressCep").HasMaxLength(10);
            a.Property(x => x.Logradouro).HasColumnName("AddressLogradouro").HasMaxLength(200);
            a.Property(x => x.Numero).HasColumnName("AddressNumero").HasMaxLength(20);
            a.Property(x => x.Complemento).HasColumnName("AddressComplemento").HasMaxLength(100);
            a.Property(x => x.Bairro).HasColumnName("AddressBairro").HasMaxLength(100);
            a.Property(x => x.Cidade).HasColumnName("AddressCidade").HasMaxLength(100);
            a.Property(x => x.Uf).HasColumnName("AddressUf").HasMaxLength(2);
        });
    }
}
