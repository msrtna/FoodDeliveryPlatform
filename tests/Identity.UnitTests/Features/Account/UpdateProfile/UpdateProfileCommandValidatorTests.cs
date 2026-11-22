using Identity.Application.Features.Account.UpdateProfile;

namespace Identity.UnitTests.Features.Account.UpdateProfile;

public sealed class UpdateProfileCommandValidatorTests
{
    private readonly UpdateProfileCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_IsValid()
    {
        var command = new UpdateProfileCommand(
            "user-123",
            "John",
            "Doe");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Doe")]
    [InlineData("John", "")]
    [InlineData(" ", "Doe")]
    [InlineData("John", " ")]
    public void Validate_WithMissingName_IsInvalid(
        string firstName,
        string lastName)
    {
        var command = new UpdateProfileCommand(
            "user-123",
            firstName,
            lastName);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyUserId_IsInvalid()
    {
        var command = new UpdateProfileCommand(
            "",
            "John",
            "Doe");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithNameLongerThan100Characters_IsInvalid()
    {
        var command = new UpdateProfileCommand(
            "user-123",
            new string('A', 101),
            "Doe");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }
}