namespace ListLift.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public string CurrencyCode { get; }

    private Money(decimal amount, string currencyCode)
    {
        Amount = amount;
        CurrencyCode = currencyCode;
    }

    public static Money Create(decimal amount, string currencyCode)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Price must be greater than zero.");
        }

        var normalizedCurrency = currencyCode?.Trim().ToUpperInvariant();
        if (normalizedCurrency is null || normalizedCurrency.Length != 3 || normalizedCurrency.Any(character => character is < 'A' or > 'Z'))
        {
            throw new ArgumentException("Currency must be a three-letter code.", nameof(currencyCode));
        }

        return new Money(amount, normalizedCurrency);
    }
}
