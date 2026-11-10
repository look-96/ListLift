using System.ComponentModel.DataAnnotations;
using ListLift.Domain.Constants;

namespace ListLift.Application.ListingDrafts.Commands;

public sealed record SearchMarketplaceCategoriesQuery(
    MarketplaceChannel Channel,
    [property: Required, StringLength(100, MinimumLength = 2)] string Query);
