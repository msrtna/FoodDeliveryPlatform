namespace Identity.Api.Models;

public sealed record ChangeEmailRequest(
    string NewEmail,
    string CurrentPassword);