using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ListLift.Application.Abstractions;

namespace ListLift.Infrastructure.Photos;

internal sealed class AzureBlobListingPhotoStorage(BlobContainerClient containerClient) : IListingPhotoStorage
{
    public async Task<string> StoreAsync(
        ReadOnlyMemory<byte> content,
        string mediaType,
        CancellationToken cancellationToken)
    {
        if (content.IsEmpty)
            throw new ArgumentException("Photo content cannot be empty.", nameof(content));

        var extension = GetExtension(mediaType);
        var storageKey = $"{Guid.NewGuid():N}{extension}";
        var blob = containerClient.GetBlobClient(storageKey);

        await blob.UploadAsync(
            new BinaryData(content.ToArray()),
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = mediaType },
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }
            },
            cancellationToken);

        return storageKey;
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        var blob = containerClient.GetBlobClient(ValidateStorageKey(storageKey));
        await blob.DeleteIfExistsAsync(
            DeleteSnapshotsOption.IncludeSnapshots,
            cancellationToken: cancellationToken);
    }

    private static string GetExtension(string mediaType) => mediaType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => throw new ArgumentException("Unsupported photo media type.", nameof(mediaType))
    };

    private static string ValidateStorageKey(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || storageKey.Length is < 36 or > 37)
            throw new ArgumentException("Invalid photo storage key.", nameof(storageKey));

        var extension = Path.GetExtension(storageKey);
        var name = Path.GetFileNameWithoutExtension(storageKey);
        if (!Guid.TryParseExact(name, "N", out _) || extension is not (".jpg" or ".png" or ".webp"))
            throw new ArgumentException("Invalid photo storage key.", nameof(storageKey));

        return storageKey;
    }
}
