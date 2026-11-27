
using MediatR;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.DTOs.MenuCategories;

namespace Restaurant.Application.Features.MenuCategories.Queries;

public sealed class GetMenuCategoriesByRestaurantIdQueryHandler
    : IRequestHandler<
        GetMenuCategoriesByRestaurantIdQuery,
        IReadOnlyCollection<MenuCategoryDto>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMenuCategoryRepository _menuCategoryRepository;

    public GetMenuCategoriesByRestaurantIdQueryHandler(
        IRestaurantRepository restaurantRepository,
        IMenuCategoryRepository menuCategoryRepository)
    {
        _restaurantRepository = restaurantRepository;
        _menuCategoryRepository = menuCategoryRepository;
    }

    public async Task<IReadOnlyCollection<MenuCategoryDto>> Handle(
        GetMenuCategoriesByRestaurantIdQuery request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(
            request.RestaurantId,
            cancellationToken);

        if (restaurant is null)
        {
            throw new KeyNotFoundException(
                $"Restaurant with ID '{request.RestaurantId}' was not found.");
        }

        var categories = await _menuCategoryRepository.GetByRestaurantIdAsync(
            request.RestaurantId,
            request.IsActive,
            cancellationToken);

        return categories
            .Select(category => new MenuCategoryDto(
                category.Id,
                category.RestaurantId,
                category.Name,
                category.Description,
                category.DisplayOrder,
                category.IsActive))
            .ToArray();
    }
}
