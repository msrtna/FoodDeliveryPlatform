using Restaurant.Domain.Entities;
using Restaurant.Domain.Exceptions;

namespace Restaurant.UnitTests.Domain.Entities;

public sealed class MenuItemTests
{
    private readonly Guid _categoryId = Guid.CreateVersion7();

    [Fact]
    public void Constructor_ShouldCreateMenuItem_WhenInputIsValid()
    {
        var item = new MenuItem(
            _categoryId,
            "  Pizza  ",
            15.50m,
            "  Cheese pizza  ",
            1);

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(_categoryId, item.MenuCategoryId);
        Assert.Equal("Pizza", item.Name);
        Assert.Equal(15.50m, item.Price);
        Assert.Equal("Cheese pizza", item.Description);
        Assert.Equal(1, item.DisplayOrder);
        Assert.True(item.IsAvailable);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCategoryIdIsEmpty()
    {
        Assert.Throws<DomainException>(() =>
            new MenuItem(Guid.Empty, "Pizza", 10m));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNameIsEmpty()
    {
        Assert.Throws<DomainException>(() =>
            new MenuItem(_categoryId, " ", 10m));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenPriceIsNegative()
    {
        Assert.Throws<DomainException>(() =>
            new MenuItem(_categoryId, "Pizza", -1m));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDisplayOrderIsNegative()
    {
        Assert.Throws<DomainException>(() =>
            new MenuItem(_categoryId, "Pizza", 10m, displayOrder: -1));
    }

    [Fact]
    public void Update_ShouldChangeMenuItemProperties()
    {
        var item = new MenuItem(_categoryId, "Pizza", 10m);

        item.Update("Burger", 12.50m, "Beef burger", 2);

        Assert.Equal("Burger", item.Name);
        Assert.Equal(12.50m, item.Price);
        Assert.Equal("Beef burger", item.Description);
        Assert.Equal(2, item.DisplayOrder);
    }

    [Fact]
    public void SetUnavailable_ShouldMarkItemAsUnavailable()
    {
        var item = new MenuItem(_categoryId, "Pizza", 10m);

        item.SetUnavailable();

        Assert.False(item.IsAvailable);
    }

    [Fact]
    public void SetAvailable_ShouldMarkItemAsAvailable()
    {
        var item = new MenuItem(_categoryId, "Pizza", 10m);
        item.SetUnavailable();

        item.SetAvailable();

        Assert.True(item.IsAvailable);
    }

    [Fact]
    public void Constructor_ShouldAllowZeroPrice()
    {
        var item = new MenuItem(_categoryId, "Free item", 0m);

        Assert.Equal(0m, item.Price);
    }
}