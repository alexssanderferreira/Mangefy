using Mangefy.Domain.Owners;
using Mangefy.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OwnerId).IsRequired();
        builder.HasIndex(x => x.OwnerId);
        builder.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.Slug).IsUnique();

        builder.OwnsOne(x => x.Email, e => e.Property(x => x.Value).HasColumnName("Email").HasMaxLength(200));
        builder.OwnsOne(x => x.Phone, p => p.Property(x => x.Value).HasColumnName("Phone").HasMaxLength(20));

        builder.OwnsOne(x => x.Address, a =>
        {
            a.Property(x => x.Cep).HasColumnName("Address_Cep").HasMaxLength(8);
            a.Property(x => x.Logradouro).HasColumnName("Address_Logradouro").HasMaxLength(300);
            a.Property(x => x.Numero).HasColumnName("Address_Numero").HasMaxLength(20);
            a.Property(x => x.Complemento).HasColumnName("Address_Complemento").HasMaxLength(100);
            a.Property(x => x.Bairro).HasColumnName("Address_Bairro").HasMaxLength(100);
            a.Property(x => x.Cidade).HasColumnName("Address_Cidade").HasMaxLength(100);
            a.Property(x => x.Uf).HasColumnName("Address_Uf").HasMaxLength(2);
        });

        builder.Property(x => x.Timezone).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
    }
}
