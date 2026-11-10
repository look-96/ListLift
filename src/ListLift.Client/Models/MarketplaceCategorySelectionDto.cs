namespace ListLift.Client.Models;

public sealed record MarketplaceCategorySelectionDto(
    int Channel,
    string CategoryId,
    string CategoryName,
    string CategoryPath,
    DateTimeOffset SelectedAtUtc);
