using MediatR;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.DTOs.MenuItems;

namespace Restaurant.Application.Features.MenuItems.Commands;

public sealed class SetMenuItemAvailabilityCommandHandler(
    IMenuCategoryRepository menuCategoryRepository,
    IRestaurantRepository restaurantRepository)
    : IRequestHandler<SetMenuItemAvailabilityCommand, MenuItemDto>
{
    public async Task<MenuItemDto> Handle(
        SetMenuItemAvailabilityCommand request,
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

        if (request.IsAvailable)
            menuItem.SetAvailable();
        else
            menuItem.SetUnavailable();

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