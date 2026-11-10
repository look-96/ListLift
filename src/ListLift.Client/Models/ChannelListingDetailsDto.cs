namespace ListLift.Client.Models;

public sealed record ChannelListingDetailsDto(
    Guid Id,
    int Channel,
    int Status,
    string? ExternalListingId,
    string? SafeErrorCode,
    DateTimeOffset UpdatedAtUtc);
