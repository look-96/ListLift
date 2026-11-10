using ListLift.Application.Abstractions;
using Microsoft.Extensions.Hosting;

namespace ListLift.Infrastructure.Photos;

internal sealed class PrivateFileListingPhotoStorage : IListingPhotoStorage
{
    private readonly string _storageRoot;

    public PrivateFileListingPhotoStorage(IHostEnvironment environment)
    {
        _storageRoot = Path.GetFullPath(Path.Combine(
            environment.ContentRootPath,
            "App_Data",
            "ListingPhotos"));
        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<string> StoreAsync(
        ReadOnlyMemory<byte> content,
        string mediaType,
        CancellationToken cancellationToken)
    {
        if (content.IsEmpty)
            throw new ArgumentException("Photo content cannot be empty.", nameof(content));

        var extension = mediaType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new ArgumentException("Unsupported photo media type.", nameof(mediaType))
        };

        var storageKey = $"{Guid.NewGuid():N}{extension}";
        var targetPath = ResolveStorageKey(storageKey);
        var temporaryPath = Path.Combine(_storageRoot, $"{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var target = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 64 * 1024,
                options: FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await target.WriteAsync(content, cancellationToken);
                await target.FlushAsync(cancellationToken);
            }

            File.Move(temporaryPath, targetPath);
            return storageKey;
        }
        catch
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
            throw;
        }
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = ResolveStorageKey(storageKey);
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    private string ResolveStorageKey(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || storageKey.Length is < 36 or > 37)
            throw new ArgumentException("Invalid photo storage key.", nameof(storageKey));

        var extension = Path.GetExtension(storageKey);
        var name = Path.GetFileNameWithoutExtension(storageKey);
        if (!Guid.TryParseExact(name, "N", out _) || extension is not (".jpg" or ".png" or ".webp"))
            throw new ArgumentException("Invalid photo storage key.", nameof(storageKey));

        var fullPath = Path.GetFullPath(Path.Combine(_storageRoot, storageKey));
        var rootWithSeparator = _storageRoot.EndsWith(Path.DirectorySeparatorChar)
            ? _storageRoot
            : _storageRoot + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid photo storage key.", nameof(storageKey));

        return fullPath;
    }
}
