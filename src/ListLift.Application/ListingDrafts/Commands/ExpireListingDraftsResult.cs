namespace ListLift.Application.ListingDrafts.Commands;

public sealed record ExpireListingDraftsResult(
    int DeletedDrafts,
    int FailedDrafts,
    bool MayHaveMoreExpiredDrafts);
