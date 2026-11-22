using Identity.Application.Abstractions.Identity;

namespace Identity.Application.Features.Authentication.Register;

public sealed class RegisterHandler
{
    private readonly IIdentityService _identityService;

    public RegisterHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<IdentityOperationResult> HandleAsync(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        return _identityService.RegisterAsync(
            command.FirstName,
            command.LastName,
            command.Email,
            command.Password,
            cancellationToken);
    }
}