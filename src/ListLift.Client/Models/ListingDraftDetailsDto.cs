namespace ListLift.Client.Models;

public sealed record ListingDraftDetailsDto(
    Guid Id,
    string ProductName,
    string Category,
    string ConditionDescription,
    string? VerifiedAttributes,
    string? Description,
    decimal PriceAmount,
    string CurrencyCode,
    int Quantity,
    int Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    IReadOnlyList<ListingPhotoDetailsDto> Photos,
    IReadOnlyList<ChannelListingDetailsDto> ChannelListings,
    IReadOnlyList<MarketplaceCategorySelectionDto> CategorySelections);
