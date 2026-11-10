using ListLift.Domain.Constants;

namespace ListLift.Application.ListingDrafts.Models;

public sealed record ChannelListingDetails(
    Guid Id,
    MarketplaceChannel Channel,
    ChannelListingStatus Status,
    string? ExternalListingId,
    string? SafeErrorCode,
    DateTimeOffset UpdatedAtUtc);
