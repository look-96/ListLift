using ListLift.Domain.Constants;

namespace ListLift.Application.ListingDrafts.Models;

public sealed record ListingDraftDetails(
    Guid Id,
    string ProductName,
    string Category,
    string ConditionDescription,
    string? VerifiedAttributes,
    string? Description,
    decimal PriceAmount,
    string CurrencyCode,
    int Quantity,
    ListingDraftStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    IReadOnlyList<ListingPhotoDetails> Photos,
    IReadOnlyList<ChannelListingDetails> ChannelListings,
    IReadOnlyList<ListingCategorySelectionDetails> CategorySelections);
