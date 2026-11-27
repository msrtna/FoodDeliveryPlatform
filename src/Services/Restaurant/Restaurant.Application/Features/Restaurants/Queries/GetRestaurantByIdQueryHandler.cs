using MediatR;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.DTOs.Restaurants;

namespace Restaurant.Application.Features.Restaurants.Queries;

public sealed class GetRestaurantByIdQueryHandler
    : IRequestHandler<GetRestaurantByIdQuery, RestaurantDto?>
{
    private readonly IRestaurantRepository _restaurantRepository;

    public GetRestaurantByIdQueryHandler(
        IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;
    }

    public async Task<RestaurantDto?> Handle(
        GetRestaurantByIdQuery request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (restaurant is null)
        {
            return null;
        }

        return new RestaurantDto(
            restaurant.Id,
            restaurant.Name,
            restaurant.Description,
            restaurant.Address.Province,
            restaurant.Address.City,
            restaurant.Address.StreetAddress,
            restaurant.Address.PostalCode,
            restaurant.IsActive,
            restaurant.CreatedAtUtc);
    }
}
