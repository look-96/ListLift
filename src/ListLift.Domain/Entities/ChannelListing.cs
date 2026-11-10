using ListLift.Domain.Constants;

namespace ListLift.Domain.Entities;

public sealed class ChannelListing
{
    public Guid Id { get; private set; }
    public Guid ListingDraftId { get; private set; }
    public MarketplaceChannel Channel { get; private set; }
    public ChannelListingStatus Status { get; private set; }
    public string? ExternalListingId { get; private set; }
    public string? SafeErrorCode { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private ChannelListing() { }

    public static ChannelListing Prepare(Guid draftId, MarketplaceChannel channel, DateTimeOffset nowUtc)
    {
        if (draftId == Guid.Empty) throw new ArgumentException("A draft ID is required.", nameof(draftId));
        if (!Enum.IsDefined(channel)) throw new ArgumentOutOfRangeException(nameof(channel));

        return new ChannelListing
        {
            Id = Guid.NewGuid(),
            ListingDraftId = draftId,
            Channel = channel,
            Status = ChannelListingStatus.Prepared,
            UpdatedAtUtc = nowUtc
        };
    }

    public void MarkPublishing(DateTimeOffset nowUtc)
    {
        if (Status != ChannelListingStatus.Prepared && Status != ChannelListingStatus.Failed)
            throw new InvalidOperationException("Only a prepared or failed listing can be published.");

        Status = ChannelListingStatus.Publishing;
        SafeErrorCode = null;
        UpdatedAtUtc = nowUtc;
    }

    public void MarkPublished(string externalListingId, DateTimeOffset nowUtc)
    {
        if (Status != ChannelListingStatus.Publishing)
            throw new InvalidOperationException("A listing must be publishing before it can be marked published.");
        if (string.IsNullOrWhiteSpace(externalListingId))
            throw new ArgumentException("An external listing ID is required.", nameof(externalListingId));

        ExternalListingId = externalListingId;
        Status = ChannelListingStatus.Published;
        UpdatedAtUtc = nowUtc;
    }

    public void MarkFailed(string safeErrorCode, DateTimeOffset nowUtc)
    {
        if (Status != ChannelListingStatus.Publishing)
            throw new InvalidOperationException("Only a publishing listing can be marked failed.");
        if (string.IsNullOrWhiteSpace(safeErrorCode))
            throw new ArgumentException("A safe error code is required.", nameof(safeErrorCode));

        SafeErrorCode = safeErrorCode;
        Status = ChannelListingStatus.Failed;
        UpdatedAtUtc = nowUtc;
    }
}
