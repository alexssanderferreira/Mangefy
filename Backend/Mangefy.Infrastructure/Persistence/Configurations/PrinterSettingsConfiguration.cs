using Mangefy.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class PrinterSettingsConfiguration : IEntityTypeConfiguration<PrinterSettings>
{
    public void Configure(EntityTypeBuilder<PrinterSettings> builder)
    {
        builder.ToTable("PrinterSettings");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TenantId).IsUnique();

        builder.OwnsMany(x => x.Printers, p =>
        {
            p.ToTable("Printers");
            p.HasKey(x => x.Id);
            p.Property(x => x.Name).IsRequired().HasMaxLength(100);
            p.Property(x => x.IpAddressOrPort).HasMaxLength(100);
            p.Property(x => x.Station).HasConversion<string>().HasMaxLength(30);
        });
    }
}
