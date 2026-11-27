using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.Domain.Entities;

namespace Restaurant.Infrastructure.Persistence.Configurations;

public sealed class MenuItemConfiguration
    : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("MenuItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .ValueGeneratedNever();

        builder.Property(item => item.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(item => item.Description)
            .HasMaxLength(500);

        builder.Property(item => item.Price)
            .HasPrecision(18, 2);

        builder.Property(item => item.DisplayOrder)
            .IsRequired();

        builder.Property(item => item.IsAvailable)
            .IsRequired();

        builder.HasIndex(item => item.MenuCategoryId);

        builder.HasOne<MenuCategory>()
            .WithMany(category => category.MenuItems)
            .HasForeignKey(item => item.MenuCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}