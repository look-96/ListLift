using System.Globalization;
using ListLift.Application.Abstractions;

namespace ListLift.Infrastructure.AI;

internal sealed class DemoProductDescriptionGenerator : IProductDescriptionGenerator
{
    public Task<string> GenerateAsync(ProductDescriptionInput input, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(input);

        var description = $"{input.ProductName} in {input.Category}. Price: {input.PriceAmount.ToString("0.00", CultureInfo.InvariantCulture)} {input.CurrencyCode}.";
        var confirmedDetails = new[] { input.ConditionDescription, input.VerifiedAttributes }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim());
        var details = string.Join(" ", confirmedDetails);
        if (details.Length > 0)
            description += $" {details}";

        description += " Please review the photos and product details before purchase.";
        return Task.FromResult(description.Length <= 4000 ? description : description[..4000]);
    }
}
