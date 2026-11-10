namespace ListLift.Application.Abstractions;

public sealed record ProductDescriptionInput(
    string ProductName,
    string Category,
    string ConditionDescription,
    string? VerifiedAttributes,
    decimal PriceAmount,
    string CurrencyCode);
