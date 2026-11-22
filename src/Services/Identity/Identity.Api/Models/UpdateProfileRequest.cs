namespace Identity.Api.Models;

public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName);