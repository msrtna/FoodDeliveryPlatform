using Microsoft.EntityFrameworkCore;
using Restaurant.Domain.Entities;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;

namespace Restaurant.Infrastructure.Persistence.Context;

public sealed class RestaurantDbContext : DbContext
{
    public RestaurantDbContext(
        DbContextOptions<RestaurantDbContext> options)
        : base(options)
    {
    }

    public DbSet<RestaurantEntity> Restaurants => Set<RestaurantEntity>();

    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();

    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RestaurantDbContext).Assembly);
    }
}