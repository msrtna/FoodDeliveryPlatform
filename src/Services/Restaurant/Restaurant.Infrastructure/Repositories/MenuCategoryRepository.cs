
using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Domain.Entities;
using Restaurant.Infrastructure.Persistence.Context;

namespace Restaurant.Infrastructure.Repositories;

public sealed class MenuCategoryRepository : IMenuCategoryRepository
{
    private readonly RestaurantDbContext _dbContext;

    public MenuCategoryRepository(RestaurantDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MenuCategory?> GetByIdAsync(
        Guid restaurantId,
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<MenuCategory>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                category =>
                    category.Id == categoryId &&
                    category.RestaurantId == restaurantId,
                cancellationToken);
    }

    public Task<MenuCategory?> GetByIdForUpdateAsync(
        Guid restaurantId,
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<MenuCategory>()
            .Include(category => category.MenuItems)
            .FirstOrDefaultAsync(
                category =>
                    category.Id == categoryId &&
                    category.RestaurantId == restaurantId,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<MenuCategory>> GetByRestaurantIdAsync(
        Guid restaurantId,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<MenuCategory>()
            .AsNoTracking()
            .Where(category => category.RestaurantId == restaurantId);

        if (isActive.HasValue)
        {
            query = query.Where(
                category => category.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }
}
