
namespace Restaurant.Application.DTOs.Restaurants;

public sealed record RestaurantDto(
    Guid Id,
    string Name,
    string? Description,
    string Province,
    string City,
    string StreetAddress,
    string? PostalCode,
    bool IsActive,
    DateTime CreatedAtUtc);
