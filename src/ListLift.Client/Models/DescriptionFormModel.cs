using System.ComponentModel.DataAnnotations;

namespace ListLift.Client.Models;

public sealed class DescriptionFormModel
{
    [Required, StringLength(4000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;
}
