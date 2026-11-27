using Restaurant.Domain.Entities;
using Restaurant.Domain.Exceptions;

namespace Restaurant.UnitTests.Domain.Entities;

public sealed class MenuCategoryTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesCategory()
    {
        var restaurantId = Guid.NewGuid();

        var category = new MenuCategory(
            restaurantId,
            "  Pizza  ",
            "  Different types of pizza  ",
            2);

        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal(restaurantId, category.RestaurantId);
        Assert.Equal("Pizza", category.Name);
        Assert.Equal("Different types of pizza", category.Description);
        Assert.Equal(2, category.DisplayOrder);
        Assert.True(category.IsActive);
        Assert.Empty(category.MenuItems);
    }

    [Fact]
    public void Constructor_WithEmptyRestaurantId_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new MenuCategory(Guid.Empty, "Pizza"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsDomainException(
        string? name)
    {
        Assert.Throws<DomainException>(() =>
            new MenuCategory(Guid.NewGuid(), name!));
    }

    [Fact]
    public void Constructor_WithNameLongerThan100Characters_ThrowsDomainException()
    {
        var name = new string('A', 101);

        Assert.Throws<DomainException>(() =>
            new MenuCategory(Guid.NewGuid(), name));
    }

    [Fact]
    public void Constructor_WithDescriptionLongerThan500Characters_ThrowsDomainException()
    {
        var description = new string('A', 501);

        Assert.Throws<DomainException>(() =>
            new MenuCategory(
                Guid.NewGuid(),
                "Pizza",
                description));
    }

    [Fact]
    public void Constructor_WithNegativeDisplayOrder_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new MenuCategory(
                Guid.NewGuid(),
                "Pizza",
                displayOrder: -1));
    }

    [Fact]
    public void Update_WithValidData_UpdatesCategory()
    {
        var category = CreateCategory();

        category.Update(
            "  Burgers  ",
            "  Different types of burgers  ",
            3);

        Assert.Equal("Burgers", category.Name);
        Assert.Equal("Different types of burgers", category.Description);
        Assert.Equal(3, category.DisplayOrder);
    }

    [Fact]
    public void Update_WithInvalidName_ThrowsDomainException()
    {
        var category = CreateCategory();

        Assert.Throws<DomainException>(() =>
            category.Update("", null, 0));
    }

    [Fact]
    public void Update_WithNegativeDisplayOrder_ThrowsDomainException()
    {
        var category = CreateCategory();

        Assert.Throws<DomainException>(() =>
            category.Update("Burgers", null, -1));
    }

    [Fact]
    public void Activate_WhenCategoryIsInactive_ActivatesCategory()
    {
        var category = CreateCategory();
        category.Deactivate();

        category.Activate();

        Assert.True(category.IsActive);
    }

    [Fact]
    public void Deactivate_WhenCategoryIsActive_DeactivatesCategory()
    {
        var category = CreateCategory();

        category.Deactivate();

        Assert.False(category.IsActive);
    }

    [Fact]
    public void AddMenuItem_WithValidItem_AddsItemToCategory()
    {
        var category = CreateCategory();
        var item = CreateMenuItem(category, "Margherita");

        category.AddMenuItem(item);

        Assert.Single(category.MenuItems);
        Assert.Contains(item, category.MenuItems);
    }

    [Fact]
    public void AddMenuItem_WithItemFromAnotherCategory_ThrowsDomainException()
    {
        var category = CreateCategory();
        var anotherCategory = CreateCategory();
        var item = CreateMenuItem(anotherCategory, "Margherita");

        Assert.Throws<DomainException>(() =>
            category.AddMenuItem(item));
    }

    [Fact]
    public void AddMenuItem_WithDuplicateName_ThrowsDomainException()
    {
        var category = CreateCategory();

        category.AddMenuItem(CreateMenuItem(category, "Margherita"));

        var duplicateItem = CreateMenuItem(category, "margherita");

        Assert.Throws<DomainException>(() =>
            category.AddMenuItem(duplicateItem));
    }

    [Fact]
    public void AddMenuItem_WithNullItem_ThrowsArgumentNullException()
    {
        var category = CreateCategory();

        Assert.Throws<ArgumentNullException>(() =>
            category.AddMenuItem(null!));
    }

    [Fact]
    public void RemoveMenuItem_WithExistingItem_RemovesItem()
    {
        var category = CreateCategory();
        var item = CreateMenuItem(category, "Margherita");

        category.AddMenuItem(item);

        category.RemoveMenuItem(item.Id);

        Assert.Empty(category.MenuItems);
    }

    [Fact]
    public void RemoveMenuItem_WithUnknownId_ThrowsDomainException()
    {
        var category = CreateCategory();

        Assert.Throws<DomainException>(() =>
            category.RemoveMenuItem(Guid.NewGuid()));
    }

    private static MenuCategory CreateCategory()
    {
        return new MenuCategory(
            Guid.NewGuid(),
            "Pizza",
            "Pizza category");
    }

    private static MenuItem CreateMenuItem(
        MenuCategory category,
        string name)
    {
        return new MenuItem(
            category.Id,
            name,
            10m);
    }
}