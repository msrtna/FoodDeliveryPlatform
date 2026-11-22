using FluentValidation;
using Identity.Application.Abstractions.Authentication;
using Identity.Application.Features.Authentication.Login;
using Identity.Application.Features.Authentication.Register;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();

        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();

        return services;
    }
}