using System.ComponentModel.DataAnnotations;

namespace ListLift.API.Models;

public sealed class SaveApprovedDescriptionRequest
{
    [Required, StringLength(4000, MinimumLength = 1)]
    public string Description { get; init; } = string.Empty;
}
