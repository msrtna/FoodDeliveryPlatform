using Moq;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.Features.Restaurants.Queries;
using Restaurant.Domain.ValueObjects;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;

namespace Restaurant.UnitTests.Application.Restaurants.Queries;

public sealed class GetRestaurantsQueryHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repositoryMock;
    private readonly GetRestaurantsQueryHandler _handler;

    public GetRestaurantsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IRestaurantRepository>();

        _handler = new GetRestaurantsQueryHandler(
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRestaurantsExist_ShouldReturnPagedRestaurantDtos()
    {
        // Arrange
        var restaurant = new RestaurantEntity(
            "Italian Restaurant",
            new RestaurantAddress(
                "Tehran",
                "Tehran",
                "Valiasr Street",
                "12345"),
            "Authentic Italian food");

        _repositoryMock
            .Setup(repository => repository.GetPagedAsync(
                1,
                10,
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyCollection<RestaurantEntity>)[restaurant], 21));

        var query = new GetRestaurantsQuery(
            PageNumber: 1,
            PageSize: 10,
            SearchTerm: null,
            IsActive: true);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        var item = Assert.Single(result.Items);

        Assert.Equal(restaurant.Id, item.Id);
        Assert.Equal("Italian Restaurant", item.Name);
        Assert.Equal("Authentic Italian food", item.Description);
        Assert.Equal("Tehran", item.Province);
        Assert.Equal("Tehran", item.City);
        Assert.Equal("Valiasr Street", item.StreetAddress);
        Assert.Equal("12345", item.PostalCode);
        Assert.True(item.IsActive);

        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(21, result.TotalCount);
        Assert.Equal(3, result.TotalPages);

        _repositoryMock.Verify(
            repository => repository.GetPagedAsync(
                1,
                10,
                null,
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPageNumberAndPageSizeAreOutOfRange_ShouldNormalizeThem()
    {
        // Arrange
        _repositoryMock
            .Setup(repository => repository.GetPagedAsync(
                1,
                100,
                "Pizza",
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyCollection<RestaurantEntity>)[], 0));

        var query = new GetRestaurantsQuery(
            PageNumber: 0,
            PageSize: 150,
            SearchTerm: "Pizza",
            IsActive: false);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(100, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);

        _repositoryMock.Verify(
            repository => repository.GetPagedAsync(
                1,
                100,
                "Pizza",
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}