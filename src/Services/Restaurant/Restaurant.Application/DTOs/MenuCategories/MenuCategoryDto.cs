namespace Restaurant.Application.DTOs.MenuCategories;

public sealed record MenuCategoryDto(
    Guid Id,
    Guid RestaurantId,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive);