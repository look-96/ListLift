using System.ComponentModel.DataAnnotations;

namespace ListLift.Application.ListingDrafts.Commands;

public sealed record CreateListingDraftCommand(
    [property: Required, StringLength(120, MinimumLength = 1)] string ProductName,
    [property: Required, StringLength(100, MinimumLength = 1)] string Category,
    [property: Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] decimal PriceAmount,
    [property: Required, RegularExpression("^[A-Za-z]{3}$")] string CurrencyCode,
    [property: Range(1, 999999)] int? Quantity = null);
