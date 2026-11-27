using MediatR;
using Restaurant.Application.Abstractions.Persistence;

namespace Restaurant.Application.Features.Restaurants.Commands;

public sealed class DeactivateRestaurantCommandHandler
    : IRequestHandler<DeactivateRestaurantCommand, bool>
{
    private readonly IRestaurantRepository _restaurantRepository;

    public DeactivateRestaurantCommandHandler(
        IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;
    }

    public async Task<bool> Handle(
        DeactivateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (restaurant is null)
        {
            return false;
        }

        restaurant.Deactivate();

        await _restaurantRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}