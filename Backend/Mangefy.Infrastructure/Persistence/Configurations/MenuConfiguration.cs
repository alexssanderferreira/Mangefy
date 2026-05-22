using Mangefy.Domain.Menus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mangefy.Infrastructure.Persistence.Configurations;

public sealed class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("Menus");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

        builder.OwnsOne(x => x.Schedule, s =>
        {
            s.Property(x => x.StartTime).HasColumnName("Schedule_StartTime");
            s.Property(x => x.EndTime).HasColumnName("Schedule_EndTime");
            s.Property<List<DayOfWeek>>("_activeDays")
                .HasField("_activeDays")
                .HasColumnName("Schedule_ActiveDays")
                .HasConversion(
                    v => string.Join(',', v.Select(d => (int)d)),
                    v => v.Length == 0 ? new List<DayOfWeek>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                                    .Select(x => (DayOfWeek)int.Parse(x)).ToList())
                .HasMaxLength(50)
                .Metadata.SetValueComparer(new ValueComparer<List<DayOfWeek>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
        });

        builder.OwnsMany(x => x.Categories, cat =>
        {
            cat.ToTable("MenuCategories");
            cat.HasKey(x => x.Id);
            cat.Property(x => x.Name).IsRequired().HasMaxLength(100);
            cat.Property(x => x.Description).HasMaxLength(500);
            cat.Property(x => x.IsActive);

            cat.OwnsMany(x => x.Items, item =>
            {
                item.ToTable("MenuItems");
                item.HasKey(x => x.Id);
                item.Property(x => x.Name).IsRequired().HasMaxLength(200);
                item.Property(x => x.Description).HasMaxLength(1000);
                item.Property(x => x.ImageUrl).HasMaxLength(500);
                item.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                item.Property(x => x.Station).HasConversion<string>().HasMaxLength(30);

                item.OwnsOne(x => x.Price, p =>
                {
                    p.Property(x => x.Amount).HasColumnName("Price").HasColumnType("decimal(10,2)");
                    p.Property(x => x.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
                });

                item.OwnsOne(x => x.PromotionalPrice, p =>
                {
                    p.Property(x => x.Amount).HasColumnName("PromotionalPrice").HasColumnType("decimal(10,2)");
                    p.Property(x => x.Currency).HasColumnName("PromotionalPriceCurrency").HasMaxLength(3);
                });

                item.Property(x => x.PromotionValidUntil);

                item.OwnsMany(x => x.Recipe, r =>
                {
                    r.ToTable("MenuItemRecipes");
                    r.WithOwner().HasForeignKey("MenuItemId");
                    r.HasKey("MenuItemId", nameof(RecipeIngredient.StockItemId));
                    r.Property(x => x.StockItemId).IsRequired();
                    r.Property(x => x.Quantity).HasColumnType("decimal(10,4)");
                });

                item.OwnsMany(x => x.PriceHistory, ph =>
                {
                    ph.ToTable("MenuItemPriceHistory");
                    ph.HasKey(x => x.Id);
                    ph.OwnsOne(x => x.PreviousPrice, p =>
                    {
                        p.Property(x => x.Amount).HasColumnName("PreviousPrice").HasColumnType("decimal(10,2)");
                        p.Property(x => x.Currency).HasColumnName("PreviousPriceCurrency").HasMaxLength(3);
                    });
                    ph.OwnsOne(x => x.NewPrice, p =>
                    {
                        p.Property(x => x.Amount).HasColumnName("NewPrice").HasColumnType("decimal(10,2)");
                        p.Property(x => x.Currency).HasColumnName("NewPriceCurrency").HasMaxLength(3);
                    });
                    ph.Property(x => x.Reason).HasMaxLength(500);
                });
            });
        });
    }
}
