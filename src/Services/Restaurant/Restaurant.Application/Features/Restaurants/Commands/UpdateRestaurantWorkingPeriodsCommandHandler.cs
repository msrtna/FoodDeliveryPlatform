using MediatR;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.Application.Features.Restaurants.Commands;

public sealed class UpdateRestaurantWorkingPeriodsCommandHandler
    : IRequestHandler<UpdateRestaurantWorkingPeriodsCommand, bool>
{
    private readonly IRestaurantRepository _restaurantRepository;

    public UpdateRestaurantWorkingPeriodsCommandHandler(
        IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;
    }

    public async Task<bool> Handle(
        UpdateRestaurantWorkingPeriodsCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(
            request.RestaurantId,
            cancellationToken);

        if (restaurant is null)
            return false;

        var workingPeriods = request.WorkingPeriods
            .Select(period => new WorkingPeriod(
                period.DayOfWeek,
                period.OpenTime,
                period.CloseTime))
            .ToList();

        restaurant.UpdateWorkingPeriods(workingPeriods);

        await _restaurantRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}