using Restaurant.Domain.Exceptions;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.UnitTests.Domain;

public sealed class RestaurantAddressTests
{
    [Fact]
    public void Constructor_WithValidValues_CreatesAddressAndTrimsText()
    {
        // Arrange & Act
        var address = new RestaurantAddress(
            "  Tehran  ",
            "  Tehran  ",
            "  Valiasr Street  ",
            " 12345 ");

        // Assert
        Assert.Equal("Tehran", address.Province);
        Assert.Equal("Tehran", address.City);
        Assert.Equal("Valiasr Street", address.StreetAddress);
        Assert.Equal("12345", address.PostalCode);
    }

    [Fact]
    public void Constructor_WithoutPostalCode_CreatesAddress()
    {
        // Arrange & Act
        var address = new RestaurantAddress(
            "Tehran",
            "Tehran",
            "Valiasr Street");

        // Assert
        Assert.Null(address.PostalCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyProvince_ThrowsDomainException(
        string province)
    {
        // Act
        var action = () => new RestaurantAddress(
            province,
            "Tehran",
            "Valiasr Street");

        // Assert
        Assert.Throws<DomainException>(action);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyCity_ThrowsDomainException(string city)
    {
        // Act
        var action = () => new RestaurantAddress(
            "Tehran Province",
            city,
            "Valiasr Street");

        // Assert
        Assert.Throws<DomainException>(action);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyStreetAddress_ThrowsDomainException(
        string streetAddress)
    {
        // Act
        var action = () => new RestaurantAddress(
            "Tehran Province",
            "Tehran",
            streetAddress);

        // Assert
        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Constructor_WithPostalCodeLongerThan20Characters_ThrowsDomainException()
    {
        // Arrange
        var postalCode = new string('1', 21);

        // Act
        var action = () => new RestaurantAddress(
            "Tehran Province",
            "Tehran",
            "Valiasr Street",
            postalCode);

        // Assert
        Assert.Throws<DomainException>(action);
    }
}