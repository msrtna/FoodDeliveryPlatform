using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Abstractions.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    [MinLength(32)]
    public string SecretKey { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    [Range(1, 30)]
    public int RefreshTokenExpirationDays { get; set; } = 7;
}