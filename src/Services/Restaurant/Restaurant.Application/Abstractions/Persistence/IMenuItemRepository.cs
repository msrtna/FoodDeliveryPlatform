using Restaurant.Domain.Entities;

namespace Restaurant.Application.Abstractions.Persistence;

public interface IMenuItemRepository
{
    Task<MenuItem?> GetByIdAsync(
        Guid menuCategoryId,
        Guid menuItemId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MenuItem>> GetByCategoryIdAsync(
        Guid menuCategoryId,
        bool? isAvailable = null,
        CancellationToken cancellationToken = default);
}