using System.Text.Json;
using Mangefy.Domain.Tabs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class TabConfiguration : IEntityTypeConfiguration<Tab>
{
    public void Configure(EntityTypeBuilder<Tab> builder)
    {
        builder.ToTable("Tabs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RowVersion).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();

        builder.Property(x => x.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.LocationNote).HasMaxLength(200);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Channel).HasConversion<string>().HasMaxLength(30);
        builder.OwnsOne(x => x.DiscountAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("DiscountAmount").HasColumnType("decimal(10,2)");
            m.Property(x => x.Currency).HasColumnName("DiscountAmountCurrency").HasMaxLength(3);
        });
        builder.OwnsOne(x => x.ServiceFee, m =>
        {
            m.Property(x => x.Amount).HasColumnName("ServiceFee").HasColumnType("decimal(10,2)");
            m.Property(x => x.Currency).HasColumnName("ServiceFeeCurrency").HasMaxLength(3);
        });
        builder.OwnsOne(x => x.Tip, m =>
        {
            m.Property(x => x.Amount).HasColumnName("Tip").HasColumnType("decimal(10,2)");
            m.Property(x => x.Currency).HasColumnName("TipCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(x => x.DeliveryInfo, di =>
        {
            di.Property(x => x.RecipientName).HasColumnName("DeliveryRecipientName").HasMaxLength(200);
            di.Property(x => x.Address).HasColumnName("DeliveryAddress").HasMaxLength(500);
            di.Property(x => x.Complement).HasColumnName("DeliveryComplement").HasMaxLength(200);
            di.Property(x => x.PhoneNumber).HasColumnName("DeliveryPhone").HasMaxLength(30);
            di.Property(x => x.ExternalOrderRef).HasColumnName("DeliveryExternalRef").HasMaxLength(100);
        });

        builder.OwnsMany(x => x.Orders, order =>
        {
            order.ToTable("Orders");
            order.HasKey(x => x.Id);
            order.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            order.Property(x => x.LocationNote).HasMaxLength(200);

            order.OwnsMany(x => x.Items, item =>
            {
                item.ToTable("OrderItems");
                item.HasKey(x => x.Id);
                item.Property(x => x.ItemName).IsRequired().HasMaxLength(200);
                item.Property(x => x.Notes).HasMaxLength(500);
                item.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                item.Property(x => x.Station).HasConversion<string>().HasMaxLength(30);
                item.Property(x => x.CancellationReason).HasMaxLength(500);
                item.Property(x => x.DiscountAmount).HasColumnType("decimal(10,2)");

                item.Property<List<string>>("_modifiers")
                    .HasField("_modifiers")
                    .HasColumnName("Modifiers")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                    .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                        (c1, c2) => c1!.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

                item.OwnsOne(x => x.UnitPrice, p =>
                {
                    p.Property(x => x.Amount).HasColumnName("UnitPrice").HasColumnType("decimal(10,2)");
                    p.Property(x => x.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
                });
            });
        });

        // Índice único filtrado (PostgreSQL): protege contra duas comandas abertas com mesmo número no mesmo tenant.
        // WHERE "Status" = 'Open' — incluído na migration InitialPostgres.
        builder.HasIndex(x => new { x.TenantId, x.Number })
            .HasFilter("\"Status\" = 'Open'")
            .IsUnique()
            .HasDatabaseName("IX_Tabs_TenantId_Number_Open");

        builder.OwnsMany(x => x.Payments, payment =>
        {
            payment.ToTable("TabPayments");
            payment.HasKey(x => x.Id);
            payment.Property(x => x.Method).HasConversion<string>().HasMaxLength(30);
            payment.Property(x => x.ChangeGiven).HasColumnType("decimal(10,2)");
            payment.Property(x => x.ExternalReference).HasMaxLength(200);

            payment.OwnsOne(x => x.Amount, p =>
            {
                p.Property(x => x.Amount).HasColumnName("Amount").HasColumnType("decimal(10,2)");
                p.Property(x => x.Currency).HasColumnName("Currency").HasMaxLength(3);
            });
        });
    }
}
