using System.ComponentModel.DataAnnotations;
using ListLift.Domain.Constants;

namespace ListLift.Application.ListingDrafts.Commands;

public sealed record SelectMarketplaceCategoryCommand(
    Guid DraftId,
    MarketplaceChannel Channel,
    [property: Required, StringLength(64, MinimumLength = 1)] string CategoryId,
    [property: Required, StringLength(200, MinimumLength = 1)] string CategoryName);
