namespace ListLift.Client.Models;

public sealed record ListingPhotoDetailsDto(
    Guid Id,
    string MediaType,
    long SizeBytes,
    int SortOrder,
    DateTimeOffset CreatedAtUtc);
