using MediatR;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.DTOs.MenuCategories;
using Restaurant.Domain.Entities;

namespace Restaurant.Application.Features.MenuCategories.Commands;

public sealed class CreateMenuCategoryCommandHandler
    : IRequestHandler<CreateMenuCategoryCommand, MenuCategoryDto>
{
    private readonly IRestaurantRepository _restaurantRepository;

    public CreateMenuCategoryCommandHandler(
        IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;
    }

    public async Task<MenuCategoryDto> Handle(
        CreateMenuCategoryCommand request,
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

        var category = new MenuCategory(
            restaurant.Id,
            request.Name,
            request.Description,
            request.DisplayOrder);

        restaurant.AddMenuCategory(category);

        await _restaurantRepository.SaveChangesAsync(cancellationToken);

        return new MenuCategoryDto(
            category.Id,
            category.RestaurantId,
            category.Name,
            category.Description,
            category.DisplayOrder,
            category.IsActive);
    }
}