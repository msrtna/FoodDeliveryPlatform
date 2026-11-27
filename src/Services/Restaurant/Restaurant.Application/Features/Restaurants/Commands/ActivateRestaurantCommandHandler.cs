using MediatR;
using Restaurant.Application.Abstractions.Persistence;

namespace Restaurant.Application.Features.Restaurants.Commands;

public sealed class ActivateRestaurantCommandHandler
    : IRequestHandler<ActivateRestaurantCommand, bool>
{
    private readonly IRestaurantRepository _restaurantRepository;

    public ActivateRestaurantCommandHandler(
        IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;
    }

    public async Task<bool> Handle(
        ActivateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (restaurant is null)
        {
            return false;
        }

        restaurant.Activate();

        await _restaurantRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}