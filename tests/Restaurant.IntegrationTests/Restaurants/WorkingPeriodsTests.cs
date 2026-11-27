using System.Net;
using System.Net.Http.Json;
using Restaurant.IntegrationTests.Infrastructure;

namespace Restaurant.IntegrationTests.Restaurants;

public sealed class WorkingPeriodsTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public WorkingPeriodsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UpdateWorkingPeriods_WhenRestaurantExists_ReturnsNoContentAndPersistsPeriods()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                name = "Working Hours Restaurant",
                description = "Integration test",
                province = "Tehran",
                city = "Tehran",
                streetAddress = "Valiasr Street",
                postalCode = "1234567890"
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdRestaurant = await createResponse.Content
            .ReadFromJsonAsync<CreateRestaurantResponse>();

        Assert.NotNull(createdRestaurant);

        var workingPeriods = new[]
        {
            new
            {
                dayOfWeek = DayOfWeek.Saturday,
                openTime = "09:00:00",
                closeTime = "22:00:00"
            },
            new
            {
                dayOfWeek = DayOfWeek.Sunday,
                openTime = "10:00:00",
                closeTime = "21:00:00"
            }
        };

        // Act
        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/restaurants/{createdRestaurant.Id}/working-periods",
            workingPeriods);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await _client.GetAsync(
            $"/api/restaurants/{createdRestaurant.Id}/working-periods");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var result = await getResponse.Content
            .ReadFromJsonAsync<List<WorkingPeriodResponse>>();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Contains(result, period =>
            period.DayOfWeek == DayOfWeek.Saturday &&
            period.OpenTime == TimeOnly.Parse("09:00:00") &&
            period.CloseTime == TimeOnly.Parse("22:00:00"));

        Assert.Contains(result, period =>
            period.DayOfWeek == DayOfWeek.Sunday &&
            period.OpenTime == TimeOnly.Parse("10:00:00") &&
            period.CloseTime == TimeOnly.Parse("21:00:00"));
    }

    [Fact]
    public async Task UpdateWorkingPeriods_WhenCloseTimeIsBeforeOpenTime_ReturnsBadRequest()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                name = "Invalid Hours Restaurant",
                description = "Integration test",
                province = "Tehran",
                city = "Tehran",
                streetAddress = "Valiasr Street",
                postalCode = "1234567890"
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdRestaurant = await createResponse.Content
            .ReadFromJsonAsync<CreateRestaurantResponse>();

        Assert.NotNull(createdRestaurant);

        var invalidPeriods = new[]
        {
        new
        {
            dayOfWeek = DayOfWeek.Monday,
            openTime = "22:00:00",
            closeTime = "09:00:00"
        }
    };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/restaurants/{createdRestaurant.Id}/working-periods",
            invalidPeriods);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


    private sealed record CreateRestaurantResponse(Guid Id);

    private sealed record WorkingPeriodResponse(
        DayOfWeek DayOfWeek,
        TimeOnly OpenTime,
        TimeOnly CloseTime);
}
