using Mangefy.Domain.Plans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

        builder.OwnsOne(x => x.MonthlyPrice, m =>
        {
            m.Property(x => x.Amount).HasColumnName("MonthlyPrice").HasColumnType("decimal(10,2)");
            m.Property(x => x.Currency).HasColumnName("MonthlyPriceCurrency").HasMaxLength(3);
        });
    }
}
