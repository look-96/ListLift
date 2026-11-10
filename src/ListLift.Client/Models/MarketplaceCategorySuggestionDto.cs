namespace ListLift.Client.Models;

public sealed record MarketplaceCategorySuggestionDto(
    int Channel,
    string CategoryId,
    string Name,
    string Path);
