using Mangefy.Domain.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TenantId, x.PlatformSupplierId });

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.PlatformSupplierId);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Cnpj).HasMaxLength(20);
        builder.Property(x => x.SupplierCategoryId).IsRequired();
        builder.Property(x => x.Website).HasMaxLength(300);
        builder.Property(x => x.RepresentativeName).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.IsActive).IsRequired();

        builder.OwnsOne(x => x.Email, e =>
            e.Property(x => x.Value).HasColumnName("Email").HasMaxLength(200));

        builder.OwnsOne(x => x.Phone, p =>
            p.Property(x => x.Value).HasColumnName("Phone").HasMaxLength(30));
    }
}
