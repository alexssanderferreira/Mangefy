using Mangefy.Domain.EmployeeSchedules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class EmployeeScheduleConfiguration : IEntityTypeConfiguration<EmployeeSchedule>
{
    public void Configure(EntityTypeBuilder<EmployeeSchedule> builder)
    {
        builder.ToTable("EmployeeSchedules");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.EmployeeId).IsUnique();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.EmployeeId).IsRequired();

        builder.OwnsMany(x => x.WeeklyShifts, shift =>
        {
            shift.ToTable("EmployeeDayShifts");
            shift.WithOwner().HasForeignKey("EmployeeScheduleId");
            shift.Property(x => x.DayOfWeek).HasConversion<string>().HasMaxLength(15).IsRequired();
            shift.Property(x => x.IsWorkDay).IsRequired();
            shift.Property(x => x.StartTime);
            shift.Property(x => x.EndTime);
        });
    }
}
