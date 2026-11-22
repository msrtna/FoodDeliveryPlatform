namespace Identity.Application.Features.Account.UpdateProfile;

public sealed record UpdateProfileCommand(
    string UserId,
    string FirstName,
    string LastName);