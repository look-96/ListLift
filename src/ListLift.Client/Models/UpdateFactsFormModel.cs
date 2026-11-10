using System.ComponentModel.DataAnnotations;

namespace ListLift.Client.Models;

public sealed class UpdateFactsFormModel
{
    [Required, StringLength(120, MinimumLength = 1)]
    public string ProductName { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string Category { get; set; } = string.Empty;

    [StringLength(300)]
    public string ConditionDescription { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? VerifiedAttributes { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal PriceAmount { get; set; }

    [Required, RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "Enter a three-letter currency code, such as GBP.")]
    public string CurrencyCode { get; set; } = "GBP";

    [Range(1, 999999, ErrorMessage = "Quantity must be between 1 and 999,999.")]
    public int? Quantity { get; set; } = 1;
}
