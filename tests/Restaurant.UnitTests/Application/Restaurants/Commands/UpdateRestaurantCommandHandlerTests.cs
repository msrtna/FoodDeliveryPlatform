using Moq;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.Features.Restaurants.Commands;
using Restaurant.Domain.Exceptions;
using Restaurant.Domain.ValueObjects;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;

namespace Restaurant.UnitTests.Application.Restaurants.Commands;

public sealed class UpdateRestaurantCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repositoryMock;
    private readonly UpdateRestaurantCommandHandler _handler;

    public UpdateRestaurantCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRestaurantRepository>();

        _handler = new UpdateRestaurantCommandHandler(
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRestaurantExists_ShouldUpdateAndSave()
    {
        // Arrange
        var restaurant = new RestaurantEntity(
            "Old Name",
            new RestaurantAddress(
                "Tehran",
                "Tehran",
                "Old Street"));

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurant.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        _repositoryMock
            .Setup(repository => repository.ExistsByNameAsync(
                "New Name",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new UpdateRestaurantCommand(
            restaurant.Id,
            "New Name",
            "Updated description",
            "Alborz",
            "Karaj",
            "New Street",
            "12345");

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal("New Name", restaurant.Name);
        Assert.Equal("Updated description", restaurant.Description);
        Assert.Equal("Alborz", restaurant.Address.Province);
        Assert.Equal("Karaj", restaurant.Address.City);
        Assert.Equal("New Street", restaurant.Address.StreetAddress);
        Assert.Equal("12345", restaurant.Address.PostalCode);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRestaurantDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RestaurantEntity?)null);

        var command = new UpdateRestaurantCommand(
            restaurantId,
            "New Name",
            null,
            "Tehran",
            "Tehran",
            "New Street",
            null);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.False(result);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldThrowDomainException()
    {
        // Arrange
        var restaurant = new RestaurantEntity(
            "Old Name",
            new RestaurantAddress(
                "Tehran",
                "Tehran",
                "Old Street"));

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurant.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        _repositoryMock
            .Setup(repository => repository.ExistsByNameAsync(
                "Existing Name",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new UpdateRestaurantCommand(
            restaurant.Id,
            "Existing Name",
            null,
            "Tehran",
            "Tehran",
            "New Street",
            null);

        // Act
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(
                command,
                CancellationToken.None));

        // Assert
        Assert.Equal(
            "A restaurant with this name already exists.",
            exception.Message);

        Assert.Equal("Old Name", restaurant.Name);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}