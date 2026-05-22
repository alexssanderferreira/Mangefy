using Mangefy.Domain.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable("Stocks");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TenantId).IsUnique();
        builder.Property(x => x.RowVersion).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();

        builder.OwnsMany(x => x.Items, item =>
        {
            item.ToTable("StockItems");
            item.HasKey(x => x.Id);
            item.Property(x => x.Name).IsRequired().HasMaxLength(200);
            item.Property(x => x.Unit).HasConversion<string>().HasMaxLength(20);
            item.Property(x => x.Station).HasConversion<string>().HasMaxLength(30);
            item.Property(x => x.CurrentQuantity).HasColumnType("decimal(14,4)");
            item.Property(x => x.MinimumQuantity).HasColumnType("decimal(14,4)");

            item.OwnsOne(x => x.CostPerUnit, m =>
            {
                m.Property(x => x.Amount).HasColumnName("CostPerUnit").HasColumnType("decimal(10,2)");
                m.Property(x => x.Currency).HasColumnName("CostCurrency").HasMaxLength(3);
            });
        });

        builder.OwnsMany(x => x.Movements, mov =>
        {
            mov.ToTable("StockMovements");
            mov.HasKey(x => x.Id);
            mov.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
            mov.Property(x => x.Quantity).HasColumnType("decimal(14,4)");
            mov.Property(x => x.Reason).HasMaxLength(500);
            mov.HasIndex(x => x.StockItemId);
        });
    }
}
