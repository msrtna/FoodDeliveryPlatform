using MediatR;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Domain.Exceptions;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.Application.Features.Restaurants.Commands;

public sealed class UpdateRestaurantCommandHandler
    : IRequestHandler<UpdateRestaurantCommand, bool>
{
    private readonly IRestaurantRepository _restaurantRepository;

    public UpdateRestaurantCommandHandler(
        IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;
    }

    public async Task<bool> Handle(
        UpdateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (restaurant is null)
        {
            return false;
        }

        var name = request.Name.Trim();

        if (!string.Equals(
                restaurant.Name,
                name,
                StringComparison.OrdinalIgnoreCase) &&
            await _restaurantRepository.ExistsByNameAsync(
                name,
                cancellationToken))
        {
            throw new DomainException(
                "A restaurant with this name already exists.");
        }

        var address = new RestaurantAddress(
            request.Province,
            request.City,
            request.StreetAddress,
            request.PostalCode);

        restaurant.UpdateInfo(name, request.Description);
        restaurant.UpdateAddress(address);

        await _restaurantRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}