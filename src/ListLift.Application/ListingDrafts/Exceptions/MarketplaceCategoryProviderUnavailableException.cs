namespace ListLift.Application.ListingDrafts.Exceptions;

public sealed class MarketplaceCategoryProviderUnavailableException()
    : Exception("Marketplace category lookup is not configured.");
