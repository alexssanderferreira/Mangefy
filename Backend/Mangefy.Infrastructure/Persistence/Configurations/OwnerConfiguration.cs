using Mangefy.Domain.Owners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.ToTable("Owners");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.OwnsOne(x => x.Email, e =>
        {
            e.Property(x => x.Value).HasColumnName("Email").IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Value).IsUnique();
        });

        builder.Property(x => x.PasswordHash).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.LastLoginAt);

        builder.OwnsOne(x => x.Phone, p =>
            p.Property(x => x.Value).HasColumnName("Phone").HasMaxLength(20));

        builder.Property(x => x.DocumentType).HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.DocumentNumber).HasMaxLength(20);
        builder.Property(x => x.Notes).HasMaxLength(2000);

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
    }
}
