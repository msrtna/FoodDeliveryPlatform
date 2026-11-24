using Restaurant.Domain.Exceptions;

namespace Restaurant.Domain.ValueObjects;

public sealed record RestaurantAddress
{
    public string Province { get; }

    public string City { get; }

    public string StreetAddress { get; }

    public string? PostalCode { get; }

    public RestaurantAddress(
        string province,
        string city,
        string streetAddress,
        string? postalCode = null)
    {
        Province = ValidateRequiredText(province, nameof(province), 100);
        City = ValidateRequiredText(city, nameof(city), 100);
        StreetAddress = ValidateRequiredText(
            streetAddress,
            nameof(streetAddress),
            250);

        PostalCode = ValidateOptionalText(postalCode, 20);
    }

    private static string ValidateRequiredText(
        string value,
        string fieldName,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        value = value.Trim();

        if (value.Length > maxLength)
        {
            throw new DomainException(
                $"{fieldName} cannot exceed {maxLength} characters.");
        }

        return value;
    }

    private static string? ValidateOptionalText(
        string? value,
        int maxLength)
    {
        value = value?.Trim();

        if (value?.Length > maxLength)
        {
            throw new DomainException(
                $"Postal code cannot exceed {maxLength} characters.");
        }

        return string.IsNullOrEmpty(value) ? null : value;
    }
}