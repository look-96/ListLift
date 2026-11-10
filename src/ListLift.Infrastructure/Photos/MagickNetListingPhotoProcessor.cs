using ImageMagick;
using ListLift.Application.Abstractions;
using ListLift.Application.ListingDrafts.Commands;

namespace ListLift.Infrastructure.Photos;

internal sealed class MagickNetListingPhotoProcessor : IListingPhotoProcessor
{
    private const int MaxDimension = 8000;
    private const ulong MaxPixelCount = 20_000_000;
    private const ulong MaxMemoryBytes = 256 * 1024 * 1024;
    private const ulong MaxProcessingSeconds = 15;

    static MagickNetListingPhotoProcessor()
    {
        ResourceLimits.Width = MaxDimension;
        ResourceLimits.Height = MaxDimension;
        ResourceLimits.Area = MaxPixelCount;
        ResourceLimits.Memory = MaxMemoryBytes;
        ResourceLimits.Disk = 0;
        ResourceLimits.Time = MaxProcessingSeconds;
        ResourceLimits.Thread = 2;
    }

    public async Task<ProcessedListingPhoto> ProcessAsync(
        Stream source,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        var bytes = await ReadBoundedAsync(source, cancellationToken);
        var (format, mediaType) = DetectSupportedFormat(bytes);

        try
        {
            using var input = new MemoryStream(bytes, writable: false);
            using var image = new MagickImage(input, format);

            if (image.Width is < 1 or > MaxDimension || image.Height is < 1 or > MaxDimension ||
                (ulong)image.Width * image.Height > MaxPixelCount)
            {
                throw new InvalidDataException("The image dimensions exceed the allowed limit.");
            }

            image.AutoOrient();
            image.Strip();
            image.Format = format;
            image.Quality = 88;

            using var output = new MemoryStream();
            image.Write(output);
            if (output.Length is < 1 or > UploadListingPhotoValidator.MaxUploadBytes)
                throw new InvalidDataException("The normalized photo exceeds the allowed size.");

            return new ProcessedListingPhoto(output.ToArray(), mediaType);
        }
        catch (MagickException exception)
        {
            throw new InvalidDataException("The uploaded file is not a valid supported image.", exception);
        }
    }

    private static (MagickFormat Format, string MediaType) DetectSupportedFormat(byte[] content)
    {
        ReadOnlySpan<byte> bytes = content;
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return (MagickFormat.Jpeg, "image/jpeg");

        ReadOnlySpan<byte> pngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        if (bytes.StartsWith(pngSignature))
            return (MagickFormat.Png, "image/png");

        if (bytes.Length >= 12 &&
            bytes[..4].SequenceEqual("RIFF"u8) &&
            bytes.Slice(8, 4).SequenceEqual("WEBP"u8))
        {
            return (MagickFormat.WebP, "image/webp");
        }

        throw new InvalidDataException("Only JPEG, PNG, and WebP image files are accepted.");
    }

    private static async Task<byte[]> ReadBoundedAsync(
        Stream source,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024];
        await using var destination = new MemoryStream();

        while (true)
        {
            var bytesRead = await source.ReadAsync(buffer, cancellationToken);
            if (bytesRead == 0)
                break;
            if (destination.Length + bytesRead > UploadListingPhotoValidator.MaxUploadBytes)
                throw new InvalidDataException("The uploaded photo exceeds the allowed size.");

            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        }

        return destination.ToArray();
    }
}
