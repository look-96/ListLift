namespace ListLift.Application.ListingDrafts.Models;

public sealed record ListingPhotoDetails(
    Guid Id,
    string MediaType,
    long SizeBytes,
    int SortOrder,
    DateTimeOffset CreatedAtUtc);
