using MediatR;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.DTOs.MenuItems;

namespace Restaurant.Application.Features.MenuItems.Queries;

public sealed class GetMenuItemByIdQueryHandler(
    IMenuCategoryRepository menuCategoryRepository,
    IMenuItemRepository menuItemRepository)
    : IRequestHandler<GetMenuItemByIdQuery, MenuItemDto>
{
    public async Task<MenuItemDto> Handle(
        GetMenuItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await menuCategoryRepository.GetByIdAsync(
            request.RestaurantId,
            request.MenuCategoryId,
            cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Menu category with ID '{request.MenuCategoryId}' was not found.");
        }

        var menuItem = await menuItemRepository.GetByIdAsync(
            request.MenuCategoryId,
            request.MenuItemId,
            cancellationToken);

        if (menuItem is null)
        {
            throw new KeyNotFoundException(
                $"Menu item with ID '{request.MenuItemId}' was not found.");
        }

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