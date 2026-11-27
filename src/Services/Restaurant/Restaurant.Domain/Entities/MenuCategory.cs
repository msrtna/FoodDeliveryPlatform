using Restaurant.Domain.Common;
using Restaurant.Domain.Exceptions;

namespace Restaurant.Domain.Entities;

public sealed class MenuCategory : BaseEntity
{
    private readonly List<MenuItem> _menuItems = [];

    public Guid RestaurantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<MenuItem> MenuItems => _menuItems.AsReadOnly();

    private MenuCategory()
    {
    }

    public MenuCategory(
        Guid restaurantId,
        string name,
        string? description = null,
        int displayOrder = 0)
    {
        if (restaurantId == Guid.Empty)
            throw new DomainException("Restaurant ID is required.");

        RestaurantId = restaurantId;
        Name = ValidateName(name);
        Description = ValidateDescription(description);
        DisplayOrder = ValidateDisplayOrder(displayOrder);
        IsActive = true;
    }

    public void Update(
        string name,
        string? description,
        int displayOrder)
    {
        Name = ValidateName(name);
        Description = ValidateDescription(description);
        DisplayOrder = ValidateDisplayOrder(displayOrder);
    }

    public void AddMenuItem(MenuItem menuItem)
    {
        ArgumentNullException.ThrowIfNull(menuItem);

        if (menuItem.MenuCategoryId != Id)
            throw new DomainException(
                "Menu item does not belong to this category.");

        if (_menuItems.Any(item =>
                string.Equals(
                    item.Name,
                    menuItem.Name,
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException(
                "A menu item with this name already exists in the category.");
        }

        _menuItems.Add(menuItem);
    }

    public void RemoveMenuItem(Guid menuItemId)
    {
        var menuItem = _menuItems.FirstOrDefault(
            item => item.Id == menuItemId);

        if (menuItem is null)
            throw new DomainException(
                "Menu item was not found in this category.");

        _menuItems.Remove(menuItem);
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required.");

        name = name.Trim();

        if (name.Length > 100)
            throw new DomainException(
                "Category name cannot exceed 100 characters.");

        return name;
    }

    private static string? ValidateDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        description = description.Trim();

        if (description.Length > 500)
            throw new DomainException(
                "Category description cannot exceed 500 characters.");

        return description;
    }

    private static int ValidateDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new DomainException(
                "Display order cannot be negative.");

        return displayOrder;
    }

    public void UpdateMenuItem(
    Guid menuItemId,
    string name,
    decimal price,
    string? description,
    int displayOrder)
    {
        var menuItem = _menuItems.FirstOrDefault(
            item => item.Id == menuItemId);

        if (menuItem is null)
            throw new DomainException(
                "Menu item was not found in this category.");

        if (_menuItems.Any(item =>
                item.Id != menuItemId &&
                string.Equals(
                    item.Name,
                    name?.Trim(),
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException(
                "A menu item with this name already exists in the category.");
        }

        menuItem.Update(name, price, description, displayOrder);
    }
}