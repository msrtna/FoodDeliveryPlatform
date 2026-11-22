using Identity.Application.Abstractions.Identity;

namespace Identity.Application.Features.Account.ChangeEmail;

public sealed class ChangeEmailHandler
{
    private readonly IIdentityService _identityService;

    public ChangeEmailHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<IdentityOperationResult> HandleAsync(
        ChangeEmailCommand command,
        CancellationToken cancellationToken)
    {
        return _identityService.ChangeEmailAsync(
            command.UserId,
            command.NewEmail,
            command.CurrentPassword,
            cancellationToken);
    }
}