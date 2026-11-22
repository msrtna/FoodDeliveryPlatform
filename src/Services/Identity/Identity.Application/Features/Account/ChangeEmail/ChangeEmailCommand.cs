namespace Identity.Application.Features.Account.ChangeEmail;

public sealed record ChangeEmailCommand(
    string UserId,
    string NewEmail,
    string CurrentPassword);