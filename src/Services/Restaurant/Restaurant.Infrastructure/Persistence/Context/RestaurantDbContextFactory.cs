
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Restaurant.Infrastructure.Persistence.Context;

public sealed class RestaurantDbContextFactory
    : IDesignTimeDbContextFactory<RestaurantDbContext>
{
    public RestaurantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<RestaurantDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=FoodDeliveryPlatform_Restaurant;Trusted_Connection=True;TrustServerCertificate=True;");

        return new RestaurantDbContext(optionsBuilder.Options);
    }
}
