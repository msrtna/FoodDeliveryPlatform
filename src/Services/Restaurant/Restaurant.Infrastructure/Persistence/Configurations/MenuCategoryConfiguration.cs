
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.Domain.Entities;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;

namespace Restaurant.Infrastructure.Persistence.Configurations;

public sealed class MenuCategoryConfiguration
    : IEntityTypeConfiguration<MenuCategory>
{
    public void Configure(EntityTypeBuilder<MenuCategory> builder)
    {
        builder.ToTable("MenuCategories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id)
            .ValueGeneratedNever();

        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(category => category.Description)
            .HasMaxLength(500);

        builder.Property(category => category.DisplayOrder)
            .IsRequired();

        builder.Property(category => category.IsActive)
            .IsRequired();

        builder.Property(category => category.RestaurantId)
            .IsRequired();

        builder.HasOne<RestaurantEntity>()
            .WithMany(restaurant => restaurant.MenuCategories)
            .HasForeignKey(category => category.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(category => new
        {
            category.RestaurantId,
            category.Name
        })
            .IsUnique();
    }
}
