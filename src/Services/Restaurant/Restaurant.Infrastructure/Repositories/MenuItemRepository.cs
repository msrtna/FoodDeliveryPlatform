using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Domain.Entities;
using Restaurant.Infrastructure.Persistence.Context;

namespace Restaurant.Infrastructure.Repositories;

public sealed class MenuItemRepository(
    RestaurantDbContext dbContext)
    : IMenuItemRepository
{
    public async Task<MenuItem?> GetByIdAsync(
        Guid menuCategoryId,
        Guid menuItemId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.MenuItems
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.MenuCategoryId == menuCategoryId
                        && item.Id == menuItemId,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<MenuItem>> GetByCategoryIdAsync(
        Guid menuCategoryId,
        bool? isAvailable = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.MenuItems
            .AsNoTracking()
            .Where(item => item.MenuCategoryId == menuCategoryId);

        if (isAvailable.HasValue)
        {
            query = query.Where(
                item => item.IsAvailable == isAvailable.Value);
        }

        return await query
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Name)
            .ToListAsync(cancellationToken);
    }
}