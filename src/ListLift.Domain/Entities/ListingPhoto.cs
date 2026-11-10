namespace ListLift.Domain.Entities;

public sealed class ListingPhoto
{
    public Guid Id { get; private set; }
    public Guid ListingDraftId { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string MediaType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public int SortOrder { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private ListingPhoto() { }

    public static ListingPhoto Create(
        Guid draftId,
        string storageKey,
        string mediaType,
        long sizeBytes,
        int sortOrder,
        DateTimeOffset createdAtUtc)
    {
        if (draftId == Guid.Empty) throw new ArgumentException("A draft ID is required.", nameof(draftId));
        if (string.IsNullOrWhiteSpace(storageKey)) throw new ArgumentException("A storage key is required.", nameof(storageKey));
        if (mediaType is not ("image/jpeg" or "image/png" or "image/webp")) throw new ArgumentException("Unsupported image media type.", nameof(mediaType));
        if (sizeBytes <= 0) throw new ArgumentOutOfRangeException(nameof(sizeBytes));
        if (sortOrder < 0) throw new ArgumentOutOfRangeException(nameof(sortOrder));

        return new ListingPhoto
        {
            Id = Guid.NewGuid(),
            ListingDraftId = draftId,
            StorageKey = storageKey,
            MediaType = mediaType,
            SizeBytes = sizeBytes,
            SortOrder = sortOrder,
            CreatedAtUtc = createdAtUtc
        };
    }
}
