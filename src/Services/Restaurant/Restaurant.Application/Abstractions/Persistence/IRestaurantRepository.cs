
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;

namespace Restaurant.Application.Abstractions.Persistence;

public interface IRestaurantRepository
{
    Task AddAsync(
        RestaurantEntity restaurant,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<RestaurantEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
