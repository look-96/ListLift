using ListLift.Domain.Constants;

namespace ListLift.Application.Abstractions;

public sealed record MarketplaceCategorySuggestion(
    MarketplaceChannel Channel,
    string CategoryId,
    string Name,
    string Path);
