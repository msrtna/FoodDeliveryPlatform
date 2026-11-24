
using Moq;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.Features.Restaurants.Commands;
using Restaurant.Domain.Exceptions;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;

namespace Restaurant.UnitTests.Application.Restaurants.Commands;

public sealed class CreateRestaurantCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repositoryMock;
    private readonly CreateRestaurantCommandHandler _handler;

    public CreateRestaurantCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRestaurantRepository>();

        _handler = new CreateRestaurantCommandHandler(
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRestaurantIsValid_ShouldCreateRestaurant()
    {
        // Arrange
        var command = new CreateRestaurantCommand(
            "Italian Restaurant",
            "Authentic Italian food",
            "Tehran",
            "Tehran",
            "Valiasr Street",
            "1234567890");

        _repositoryMock
            .Setup(repository => repository.ExistsByNameAsync(
                command.Name,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        RestaurantEntity? capturedRestaurant = null;

        _repositoryMock
            .Setup(repository => repository.AddAsync(
                It.IsAny<RestaurantEntity>(),
                It.IsAny<CancellationToken>()))
            .Callback<RestaurantEntity, CancellationToken>(
                (restaurant, _) => capturedRestaurant = restaurant)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(capturedRestaurant);
        Assert.Equal(result, capturedRestaurant.Id);
        Assert.Equal("Italian Restaurant", capturedRestaurant.Name);
        Assert.Equal("Authentic Italian food", capturedRestaurant.Description);
        Assert.Equal("Tehran", capturedRestaurant.Address.Province);
        Assert.Equal("Tehran", capturedRestaurant.Address.City);
        Assert.Equal("Valiasr Street", capturedRestaurant.Address.StreetAddress);
        Assert.Equal("1234567890", capturedRestaurant.Address.PostalCode);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<RestaurantEntity>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRestaurantNameAlreadyExists_ShouldThrowDomainException()
    {
        // Arrange
        var command = new CreateRestaurantCommand(
            "Italian Restaurant",
            null,
            "Tehran",
            "Tehran",
            "Valiasr Street",
            null);

        _repositoryMock
            .Setup(repository => repository.ExistsByNameAsync(
                command.Name,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal(
            "A restaurant with this name already exists.",
            exception.Message);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<RestaurantEntity>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAddressIsInvalid_ShouldThrowDomainException()
    {
        // Arrange
        var command = new CreateRestaurantCommand(
            "Italian Restaurant",
            null,
            "",
            "Tehran",
            "Valiasr Street",
            null);

        _repositoryMock
            .Setup(repository => repository.ExistsByNameAsync(
                command.Name,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(command, CancellationToken.None));

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<RestaurantEntity>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
