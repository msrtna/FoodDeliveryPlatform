using Restaurant.Domain.Common;
using Restaurant.Domain.Exceptions;

namespace Restaurant.Domain.Entities;

public sealed class MenuItem : BaseEntity
{
    public Guid MenuCategoryId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsAvailable { get; private set; }

    private MenuItem()
    {
    }

    public MenuItem(
        Guid menuCategoryId,
        string name,
        decimal price,
        string? description = null,
        int displayOrder = 0)
    {
        if (menuCategoryId == Guid.Empty)
            throw new DomainException("Menu category ID is required.");

        MenuCategoryId = menuCategoryId;
        Name = ValidateName(name);
        Description = ValidateDescription(description);
        Price = ValidatePrice(price);
        DisplayOrder = ValidateDisplayOrder(displayOrder);
        IsAvailable = true;
    }

    public void Update(
        string name,
        decimal price,
        string? description,
        int displayOrder)
    {
        Name = ValidateName(name);
        Price = ValidatePrice(price);
        Description = ValidateDescription(description);
        DisplayOrder = ValidateDisplayOrder(displayOrder);
    }

    public void SetAvailable() => IsAvailable = true;

    public void SetUnavailable() => IsAvailable = false;

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Menu item name is required.");

        name = name.Trim();

        if (name.Length > 150)
            throw new DomainException(
                "Menu item name cannot exceed 150 characters.");

        return name;
    }

    private static string? ValidateDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        description = description.Trim();

        if (description.Length > 500)
            throw new DomainException(
                "Menu item description cannot exceed 500 characters.");

        return description;
    }

    private static decimal ValidatePrice(decimal price)
    {
        if (price < 0)
            throw new DomainException(
                "Menu item price cannot be negative.");

        return price;
    }

    private static int ValidateDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new DomainException(
                "Display order cannot be negative.");

        return displayOrder;
    }
}