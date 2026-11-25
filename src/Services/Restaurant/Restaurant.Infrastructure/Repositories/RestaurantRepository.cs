using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Infrastructure.Persistence.Context;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;

namespace Restaurant.Infrastructure.Repositories;

public sealed class RestaurantRepository : IRestaurantRepository
{
    private readonly RestaurantDbContext _dbContext;

    public RestaurantRepository(RestaurantDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        RestaurantEntity restaurant,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Restaurants.AddAsync(
            restaurant,
            cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Restaurants.AnyAsync(
            restaurant => restaurant.Name == name,
            cancellationToken);
    }

    public Task<RestaurantEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Restaurants.FirstOrDefaultAsync(
            restaurant => restaurant.Id == id,
            cancellationToken);
    }

    public async Task<(IReadOnlyCollection<RestaurantEntity> Items, int TotalCount)>
        GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm,
            bool? isActive,
            CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Restaurants
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearchTerm = searchTerm.Trim();

            query = query.Where(restaurant =>
                restaurant.Name.Contains(normalizedSearchTerm));
        }

        if (isActive.HasValue)
        {
            query = query.Where(restaurant =>
                restaurant.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(restaurant => restaurant.Name)
            .ThenBy(restaurant => restaurant.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}