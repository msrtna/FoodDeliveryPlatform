using Microsoft.Extensions.DependencyInjection;
using Restaurant.Application.Features.Restaurants.Commands;

namespace Restaurant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(
                typeof(CreateRestaurantCommandHandler).Assembly));

        return services;
    }
}
