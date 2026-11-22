using Identity.Application.Abstractions.Identity;

namespace Identity.UnitTests.Abstractions.Identity;

public class AuthenticatedUserTests
{
    [Fact]
    public void Constructor_ShouldInitializeAllProperties()
    {
        // Arrange
        const string id = "user-123";
        const string firstName = "Mohammad";
        const string lastName = "Moradi";
        const string email = "mohammad@example.com";

        var roles = new List<string>
        {
            "Customer",
            "RestaurantOwner"
        };

        // Act
        var user = new AuthenticatedUser(
            id,
            firstName,
            lastName,
            email,
            roles);

        // Assert
        Assert.Equal(id, user.Id);
        Assert.Equal(firstName, user.FirstName);
        Assert.Equal(lastName, user.LastName);
        Assert.Equal(email, user.Email);
        Assert.Equal(roles, user.Roles);
        Assert.Equal(2, user.Roles.Count);
    }
}