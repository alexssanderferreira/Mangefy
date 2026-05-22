using Mangefy.Domain.Settings;
using Mangefy.Domain.Tabs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class PaymentSettingsConfiguration : IEntityTypeConfiguration<PaymentSettings>
{
    public void Configure(EntityTypeBuilder<PaymentSettings> builder)
    {
        builder.ToTable("PaymentSettings");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TenantId).IsUnique();

        builder.Property<List<PaymentMethod>>("_enabledMethods")
            .HasField("_enabledMethods")
            .HasColumnName("EnabledMethods")
            .HasConversion(
                v => string.Join(',', v.Select(m => m.ToString())),
                v => v.Length == 0 ? new List<PaymentMethod>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                                    .Select(Enum.Parse<PaymentMethod>).ToList())
            .HasMaxLength(500)
            .Metadata.SetValueComparer(new ValueComparer<List<PaymentMethod>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));
    }
}
