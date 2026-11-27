using MediatR;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.DTOs.MenuItems;
using Restaurant.Domain.Entities;

namespace Restaurant.Application.Features.MenuItems.Commands;

public sealed class CreateMenuItemCommandHandler(
    IRestaurantRepository restaurantRepository,
    IMenuCategoryRepository menuCategoryRepository)
    : IRequestHandler<CreateMenuItemCommand, MenuItemDto>
{
    public async Task<MenuItemDto> Handle(
        CreateMenuItemCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await restaurantRepository.GetByIdAsync(
            request.RestaurantId,
            cancellationToken);

        if (restaurant is null)
            throw new KeyNotFoundException(
                $"Restaurant with ID '{request.RestaurantId}' was not found.");

        var category = await menuCategoryRepository.GetByIdForUpdateAsync(
            request.RestaurantId,
            request.MenuCategoryId,
            cancellationToken);

        if (category is null)
            throw new KeyNotFoundException(
                $"Menu category with ID '{request.MenuCategoryId}' was not found.");

        var menuItem = new MenuItem(
            category.Id,
            request.Name,
            request.Price,
            request.Description,
            request.DisplayOrder);

        category.AddMenuItem(menuItem);

        await restaurantRepository.SaveChangesAsync(cancellationToken);

        return new MenuItemDto(
            menuItem.Id,
            menuItem.MenuCategoryId,
            menuItem.Name,
            menuItem.Description,
            menuItem.Price,
            menuItem.DisplayOrder,
            menuItem.IsAvailable);
    }
}