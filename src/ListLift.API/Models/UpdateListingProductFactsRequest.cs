using System.ComponentModel.DataAnnotations;

namespace ListLift.API.Models;

public sealed class UpdateListingProductFactsRequest
{
    [Required, StringLength(120, MinimumLength = 1)]
    public string ProductName { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string Category { get; init; } = string.Empty;

    [StringLength(300)]
    public string ConditionDescription { get; init; } = string.Empty;

    [StringLength(2000)]
    public string? VerifiedAttributes { get; init; }

    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal PriceAmount { get; init; }

    [Required, RegularExpression("^[A-Za-z]{3}$")]
    public string CurrencyCode { get; init; } = string.Empty;

    [Range(1, 999999)]
    public int? Quantity { get; init; }
}
