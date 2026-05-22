using Mangefy.Domain.Fiscal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class FiscalDocumentConfiguration : IEntityTypeConfiguration<FiscalDocument>
{
    public void Configure(EntityTypeBuilder<FiscalDocument> builder)
    {
        builder.ToTable("FiscalDocuments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.TabId).IsRequired();
        builder.Property(x => x.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Environment).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.AccessKey).HasMaxLength(100);
        builder.Property(x => x.Protocol).HasMaxLength(100);
        builder.Property(x => x.RejectReason).HasMaxLength(500);
        builder.Property(x => x.CancellationReason).HasMaxLength(500);

        builder.OwnsOne(x => x.TotalAmount, m =>
        {
            m.Property(p => p.Amount).HasColumnName("TotalAmount").HasColumnType("decimal(18,2)");
            m.Property(p => p.Currency).HasColumnName("TotalCurrency").HasMaxLength(3);
        });

        builder.HasIndex(x => new { x.TenantId, x.CreatedAt });
        builder.HasIndex(x => x.TabId);
    }
}
