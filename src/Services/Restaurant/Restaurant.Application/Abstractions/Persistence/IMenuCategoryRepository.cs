using Restaurant.Domain.Entities;

namespace Restaurant.Application.Abstractions.Persistence;

public interface IMenuCategoryRepository
{
    Task<MenuCategory?> GetByIdAsync(
        Guid restaurantId,
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MenuCategory>> GetByRestaurantIdAsync(
        Guid restaurantId,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
}