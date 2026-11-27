
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;

namespace Restaurant.Infrastructure.Persistence.Configurations;

public sealed class RestaurantConfiguration
    : IEntityTypeConfiguration<RestaurantEntity>
{
    public void Configure(EntityTypeBuilder<RestaurantEntity> builder)
    {
        builder.ToTable("Restaurants");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.IsActive)
            .IsRequired();

        builder.Property(r => r.CreatedAtUtc)
            .IsRequired();

        builder.OwnsOne(r => r.Address, address =>
        {
            address.Property(a => a.Province)
                .HasColumnName("Province")
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.City)
                .HasColumnName("City")
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.StreetAddress)
                .HasColumnName("StreetAddress")
                .IsRequired()
                .HasMaxLength(250);

            address.Property(a => a.PostalCode)
                .HasColumnName("PostalCode")
                .HasMaxLength(20);
        });

        builder.OwnsMany(r => r.WorkingPeriods, periods =>
        {
            periods.ToTable("RestaurantWorkingPeriods");

            periods.WithOwner()
                .HasForeignKey("RestaurantId");

            periods.Property<int>("Id");

            periods.HasKey("Id");

            periods.Property(p => p.DayOfWeek)
                .IsRequired();

            periods.Property(p => p.OpenTime)
                .HasColumnType("time")
                .IsRequired();

            periods.Property(p => p.CloseTime)
                .HasColumnType("time")
                .IsRequired();
        });

        builder.Navigation(r => r.Address)
            .IsRequired();
    }
}
