using Identity.Application.Abstractions.Identity;
using Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Identity.UnitTests.Identity;

public class IdentityServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly IdentityService _sut;

    public IdentityServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        var userValidators = Array.Empty<IUserValidator<ApplicationUser>>();
        var passwordValidators = Array.Empty<IPasswordValidator<ApplicationUser>>();
        var normalizer = new Mock<ILookupNormalizer>();
        var errorDescriber = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            Options.Create(new IdentityOptions()),
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            normalizer.Object,
            errorDescriber.Object,
            services.Object,
            logger.Object);

        _sut = new IdentityService(_userManagerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WhenCreationSucceeds_ShouldReturnSuccess()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-123",
            Email = "mohammad@example.com",
            UserName = "mohammad@example.com"
        };

        _userManagerMock
            .Setup(manager => manager.CreateAsync(
                It.IsAny<ApplicationUser>(),
                "Password123!"))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((createdUser, _) =>
            {
                createdUser.Id = user.Id;
            });

        // Act
        var result = await _sut.RegisterAsync(
            "Mohammad",
            "Moradi",
            "mohammad@example.com",
            "Password123!",
            CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(user.Id, result.UserId);
        Assert.Empty(result.Errors);

        _userManagerMock.Verify(
            manager => manager.CreateAsync(
                It.Is<ApplicationUser>(createdUser =>
                    createdUser.FirstName == "Mohammad" &&
                    createdUser.LastName == "Moradi" &&
                    createdUser.Email == "mohammad@example.com" &&
                    createdUser.UserName == "mohammad@example.com" &&
                    createdUser.IsActive),
                "Password123!"),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenCreationFails_ShouldReturnErrors()
    {
        // Arrange
        var identityError = new IdentityError
        {
            Code = "DuplicateEmail",
            Description = "Email is already registered."
        };

        _userManagerMock
            .Setup(manager => manager.CreateAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        // Act
        var result = await _sut.RegisterAsync(
            "Mohammad",
            "Moradi",
            "mohammad@example.com",
            "Password123!",
            CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.UserId);
        var error = Assert.Single(result.Errors);
        Assert.Equal("DuplicateEmail", error.Code);
        Assert.Equal("Email is already registered.", error.Description);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync("missing@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.AuthenticateAsync(
            "missing@example.com",
            "Password123!",
            CancellationToken.None);

        // Assert
        Assert.Null(result);

        _userManagerMock.Verify(
            manager => manager.CheckPasswordAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenPasswordIsInvalid_ShouldReturnNull()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-123",
            Email = "mohammad@example.com",
            IsActive = true
        };

        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync("mohammad@example.com"))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(manager => manager.CheckPasswordAsync(user, "WrongPassword!"))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.AuthenticateAsync(
            "mohammad@example.com",
            "WrongPassword!",
            CancellationToken.None);

        // Assert
        Assert.Null(result);

        _userManagerMock.Verify(
            manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()),
            Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenCredentialsAreValid_ShouldReturnUserAndRoles()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-123",
            FirstName = "Mohammad",
            LastName = "Moradi",
            Email = "mohammad@example.com",
            IsActive = true
        };

        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync("mohammad@example.com"))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(manager => manager.CheckPasswordAsync(user, "Password123!"))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(manager => manager.GetRolesAsync(user))
            .ReturnsAsync(["Customer"]);

        // Act
        var result = await _sut.AuthenticateAsync(
            "mohammad@example.com",
            "Password123!",
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user-123", result.Id);
        Assert.Equal("Mohammad", result.FirstName);
        Assert.Equal("Moradi", result.LastName);
        Assert.Equal("mohammad@example.com", result.Email);
        Assert.Contains("Customer", result.Roles);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExistsAndIsActive_ReturnsAuthenticatedUser()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-123",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            IsActive = true
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Customer" });

        var service = new IdentityService(_userManagerMock.Object);

        // Act
        var result = await service.GetByIdAsync(
            user.Id,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john@example.com", result.Email);
        Assert.Contains("Customer", result.Roles);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        _userManagerMock
            .Setup(x => x.FindByIdAsync("unknown-user"))
            .ReturnsAsync((ApplicationUser?)null);

        var service = new IdentityService(_userManagerMock.Object);

        // Act
        var result = await service.GetByIdAsync(
            "unknown-user",
            CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}