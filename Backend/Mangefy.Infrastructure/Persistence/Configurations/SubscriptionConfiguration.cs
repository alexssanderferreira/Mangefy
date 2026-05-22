using Mangefy.Domain.Platform.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TenantId).IsUnique();

        builder.OwnsMany(x => x.Invoices, invoice =>
        {
            invoice.ToTable("Invoices");
            invoice.HasKey(x => x.Id);
            invoice.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            invoice.Property(x => x.PaymentReference).HasMaxLength(200);
            invoice.Property(x => x.Notes).HasMaxLength(1000);

            invoice.OwnsOne(x => x.Amount, m =>
            {
                m.Property(x => x.Amount).HasColumnName("Amount").HasColumnType("decimal(10,2)");
                m.Property(x => x.Currency).HasColumnName("Currency").HasMaxLength(3);
            });
        });
    }
}
