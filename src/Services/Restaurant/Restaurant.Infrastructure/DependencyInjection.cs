
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Infrastructure.Persistence.Context;
using Restaurant.Infrastructure.Repositories;

namespace Restaurant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(
            "RestaurantDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'RestaurantDatabase' was not found.");
        }

        services.AddDbContext<RestaurantDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IRestaurantRepository, RestaurantRepository>();

        return services;
    }
}
