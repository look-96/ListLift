using Google.Cloud.Storage.V1;
using ListLift.Application.Abstractions;

namespace ListLift.Infrastructure.Photos;

internal sealed class GoogleCloudStorageListingPhotoStorage(
    StorageClient storageClient,
    string bucket,
    string objectPrefix) : IListingPhotoStorage
{
    public async Task<string> StoreAsync(
        ReadOnlyMemory<byte> content,
        string mediaType,
        CancellationToken cancellationToken)
    {
        if (content.IsEmpty)
            throw new ArgumentException("Photo content cannot be empty.", nameof(content));

        var extension = GetExtension(mediaType);
        var objectName = $"{objectPrefix}/{Guid.NewGuid():N}{extension}";
        await using var stream = new MemoryStream(content.ToArray(), writable: false);
        await storageClient.UploadObjectAsync(
            bucket,
            objectName,
            mediaType,
            stream,
            new UploadObjectOptions { IfGenerationMatch = 0 },
            cancellationToken);

        return objectName;
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        var objectName = ValidateStorageKey(storageKey);
        try
        {
            await storageClient.DeleteObjectAsync(bucket, objectName, cancellationToken: cancellationToken);
        }
        catch (Google.GoogleApiException exception) when (exception.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Expiration cleanup is idempotent; a previously deleted object is already in the desired state.
        }
    }

    private string ValidateStorageKey(string storageKey)
    {
        var expectedPrefix = $"{objectPrefix}/";
        if (string.IsNullOrWhiteSpace(storageKey) || !storageKey.StartsWith(expectedPrefix, StringComparison.Ordinal))
            throw new ArgumentException("Invalid photo storage key.", nameof(storageKey));

        var filename = storageKey[expectedPrefix.Length..];
        var extension = Path.GetExtension(filename);
        var name = Path.GetFileNameWithoutExtension(filename);
        if (filename.Contains('/') || filename.Contains('\\') ||
            !Guid.TryParseExact(name, "N", out _) || extension is not (".jpg" or ".png" or ".webp"))
        {
            throw new ArgumentException("Invalid photo storage key.", nameof(storageKey));
        }

        return storageKey;
    }

    private static string GetExtension(string mediaType) => mediaType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => throw new ArgumentException("Unsupported photo media type.", nameof(mediaType))
    };
}
