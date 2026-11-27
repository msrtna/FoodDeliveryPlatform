using MediatR;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Domain.Exceptions;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.Application.Features.Restaurants.Commands;

public sealed class CreateRestaurantCommandHandler
    : IRequestHandler<CreateRestaurantCommand, Guid>
{
    private readonly IRestaurantRepository _restaurantRepository;

    public CreateRestaurantCommandHandler(
        IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;
    }

    public async Task<Guid> Handle(
        CreateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        if (await _restaurantRepository.ExistsByNameAsync(
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

        var restaurant = new Domain.Entities.Restaurant(
            name,
            address,
            request.Description);

        await _restaurantRepository.AddAsync(
            restaurant,
            cancellationToken);

        await _restaurantRepository.SaveChangesAsync(
            cancellationToken);

        return restaurant.Id;
    }
}
