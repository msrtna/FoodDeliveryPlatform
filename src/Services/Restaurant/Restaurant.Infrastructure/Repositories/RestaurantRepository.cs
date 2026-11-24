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

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
