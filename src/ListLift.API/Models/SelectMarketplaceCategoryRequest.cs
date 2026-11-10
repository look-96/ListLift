using System.ComponentModel.DataAnnotations;

namespace ListLift.API.Models;

public sealed class SelectMarketplaceCategoryRequest
{
    [Required, StringLength(64, MinimumLength = 1)]
    public string CategoryId { get; init; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 1)]
    public string CategoryName { get; init; } = string.Empty;
}
