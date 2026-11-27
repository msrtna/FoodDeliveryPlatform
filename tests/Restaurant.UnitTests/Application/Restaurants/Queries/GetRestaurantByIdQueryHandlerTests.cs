using Moq;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.Features.Restaurants.Queries;
using Restaurant.Domain.ValueObjects;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;

namespace Restaurant.UnitTests.Application.Restaurants.Queries;

public sealed class GetRestaurantByIdQueryHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repositoryMock;
    private readonly GetRestaurantByIdQueryHandler _handler;

    public GetRestaurantByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IRestaurantRepository>();

        _handler = new GetRestaurantByIdQueryHandler(
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRestaurantExists_ShouldReturnRestaurantDto()
    {
        // Arrange
        var restaurantId = Guid.CreateVersion7();

        var restaurant = new RestaurantEntity(
            "Italian Restaurant",
            new RestaurantAddress(
                "Tehran",
                "Tehran",
                "Valiasr Street",
                "1234567890"),
            "Authentic Italian food");

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        // Act
        var result = await _handler.Handle(
            new GetRestaurantByIdQuery(restaurantId),
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(restaurant.Id, result.Id);
        Assert.Equal("Italian Restaurant", result.Name);
        Assert.Equal("Authentic Italian food", result.Description);
        Assert.Equal("Tehran", result.Province);
        Assert.Equal("Tehran", result.City);
        Assert.Equal("Valiasr Street", result.StreetAddress);
        Assert.Equal("1234567890", result.PostalCode);
        Assert.True(result.IsActive);

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                restaurantId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRestaurantDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var restaurantId = Guid.CreateVersion7();

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RestaurantEntity?)null);

        // Act
        var result = await _handler.Handle(
            new GetRestaurantByIdQuery(restaurantId),
            CancellationToken.None);

        // Assert
        Assert.Null(result);

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                restaurantId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
