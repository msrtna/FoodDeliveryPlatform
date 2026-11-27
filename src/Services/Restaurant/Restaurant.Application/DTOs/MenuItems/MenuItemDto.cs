namespace Restaurant.Application.DTOs.MenuItems;

public sealed record MenuItemDto(
    Guid Id,
    Guid MenuCategoryId,
    string Name,
    string? Description,
    decimal Price,
    int DisplayOrder,
    bool IsAvailable);