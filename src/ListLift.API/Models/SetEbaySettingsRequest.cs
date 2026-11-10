using System.ComponentModel.DataAnnotations;

namespace ListLift.API.Models;

public sealed class SetEbaySettingsRequest
{
    [Required, StringLength(1000, MinimumLength = 1)]
    public string ClientId { get; init; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 1)]
    public string ClientSecret { get; init; } = string.Empty;
}
