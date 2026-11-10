using ListLift.Domain.Constants;

namespace ListLift.Application.ListingDrafts.Models;

public sealed record ListingCategorySelectionDetails(
    MarketplaceChannel Channel,
    string CategoryId,
    string CategoryName,
    string CategoryPath,
    DateTimeOffset SelectedAtUtc);
