
using Moq;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.Features.MenuCategories.Queries;
using Restaurant.Domain.Entities;

namespace Restaurant.UnitTests.Application.MenuCategories.Queries;

public sealed class GetMenuCategoryByIdQueryHandlerTests
{
    private readonly Mock<IMenuCategoryRepository> _repositoryMock = new();

    [Fact]
    public async Task Handle_WhenCategoryExists_ReturnsMenuCategory()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();

        var category = new MenuCategory(
            restaurantId,
            "Pizza",
            "Different types of pizza",
            1);

        var query = new GetMenuCategoryByIdQuery(
            restaurantId,
            category.Id);

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(restaurantId, result.RestaurantId);
        Assert.Equal("Pizza", result.Name);
        Assert.Equal("Different types of pizza", result.Description);
        Assert.Equal(1, result.DisplayOrder);
        Assert.True(result.IsActive);

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                restaurantId,
                category.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var query = new GetMenuCategoryByIdQuery(
            restaurantId,
            categoryId);

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                categoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuCategory?)null);

        var handler = CreateHandler();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenCategoryBelongsToAnotherRestaurant_ThrowsKeyNotFoundException()
    {
        // Arrange
        var requestedRestaurantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var query = new GetMenuCategoryByIdQuery(
            requestedRestaurantId,
            categoryId);

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                requestedRestaurantId,
                categoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuCategory?)null);

        var handler = CreateHandler();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(query, CancellationToken.None));
    }

    private GetMenuCategoryByIdQueryHandler CreateHandler()
    {
        return new GetMenuCategoryByIdQueryHandler(
            _repositoryMock.Object);
    }
}
