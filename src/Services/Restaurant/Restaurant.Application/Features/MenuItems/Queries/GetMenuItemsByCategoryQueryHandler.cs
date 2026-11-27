using MediatR;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.DTOs.MenuItems;

namespace Restaurant.Application.Features.MenuItems.Queries;

public sealed class GetMenuItemsByCategoryQueryHandler(
    IMenuCategoryRepository menuCategoryRepository,
    IMenuItemRepository menuItemRepository)
    : IRequestHandler<
        GetMenuItemsByCategoryQuery,
        IReadOnlyCollection<MenuItemDto>>
{
    public async Task<IReadOnlyCollection<MenuItemDto>> Handle(
        GetMenuItemsByCategoryQuery request,
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

        var menuItems = await menuItemRepository.GetByCategoryIdAsync(
            request.MenuCategoryId,
            request.IsAvailable,
            cancellationToken);

        return menuItems
            .Select(menuItem => new MenuItemDto(
                menuItem.Id,
                menuItem.MenuCategoryId,
                menuItem.Name,
                menuItem.Description,
                menuItem.Price,
                menuItem.DisplayOrder,
                menuItem.IsAvailable))
            .ToList();
    }
}