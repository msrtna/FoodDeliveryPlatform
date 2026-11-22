using Identity.Application.Abstractions.Identity;

namespace Identity.Application.Features.Account.UpdateProfile;

public sealed class UpdateProfileHandler
{
    private readonly IIdentityService _identityService;

    public UpdateProfileHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<IdentityOperationResult> HandleAsync(
        UpdateProfileCommand command,
        CancellationToken cancellationToken)
    {
        return _identityService.UpdateProfileAsync(
            command.UserId,
            command.FirstName,
            command.LastName,
            cancellationToken);
    }
}