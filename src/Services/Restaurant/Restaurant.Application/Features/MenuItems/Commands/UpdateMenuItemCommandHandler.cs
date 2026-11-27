using MediatR;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.DTOs.MenuItems;

namespace Restaurant.Application.Features.MenuItems.Commands;

public sealed class UpdateMenuItemCommandHandler(
    IMenuCategoryRepository menuCategoryRepository,
    IRestaurantRepository restaurantRepository)
    : IRequestHandler<UpdateMenuItemCommand, MenuItemDto>
{
    public async Task<MenuItemDto> Handle(
        UpdateMenuItemCommand request,
        CancellationToken cancellationToken)
    {
        var category = await menuCategoryRepository.GetByIdForUpdateAsync(
            request.RestaurantId,
            request.MenuCategoryId,
            cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Menu category with ID '{request.MenuCategoryId}' was not found.");
        }

        var menuItem = category.MenuItems.FirstOrDefault(
            item => item.Id == request.MenuItemId);

        if (menuItem is null)
        {
            throw new KeyNotFoundException(
                $"Menu item with ID '{request.MenuItemId}' was not found.");
        }

        category.UpdateMenuItem(
            request.MenuItemId,
            request.Name,
            request.Price,
            request.Description,
            request.DisplayOrder);

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