using Mangefy.Domain.BusinessSchedules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class BusinessScheduleConfiguration : IEntityTypeConfiguration<BusinessSchedule>
{
    public void Configure(EntityTypeBuilder<BusinessSchedule> builder)
    {
        builder.ToTable("BusinessSchedules");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TenantId).IsUnique();

        builder.OwnsOne(x => x.ClosingPolicy, cp =>
        {
            cp.Property(x => x.GracePeriodMinutes).HasColumnName("GracePeriodMinutes");
            cp.Property(x => x.AllowFinishOpenTabs).HasColumnName("AllowFinishOpenTabs");
            cp.Property<List<string>>("_blockedActions")
                .HasField("_blockedActions")
                .HasColumnName("BlockedActions")
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Length == 0 ? new List<string>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasMaxLength(500)
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
        });

        // DaySchedule é ValueObject — sem Id, mapeado como tabela separada via índice numérico
        builder.OwnsMany(x => x.WeeklySchedule, day =>
        {
            day.ToTable("BusinessDaySchedules");
            day.WithOwner().HasForeignKey("BusinessScheduleId");
            day.Property(x => x.DayOfWeek).HasConversion<string>().HasMaxLength(15);
            day.Property(x => x.IsOpen);
            day.Property(x => x.OpenTime);
            day.Property(x => x.CloseTime);
        });

        // SpecialDay é Entity — tem Id
        builder.OwnsMany(x => x.SpecialDays, sd =>
        {
            sd.ToTable("SpecialDays");
            sd.HasKey(x => x.Id);
            sd.WithOwner().HasForeignKey("BusinessScheduleId");
            sd.Property(x => x.Reason).IsRequired().HasMaxLength(200);
            sd.Property(x => x.IsClosed);
            sd.Property(x => x.OpenTime);
            sd.Property(x => x.CloseTime);
        });
    }
}
