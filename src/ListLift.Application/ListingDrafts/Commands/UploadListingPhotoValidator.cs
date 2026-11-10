namespace ListLift.Application.ListingDrafts.Commands;

public sealed class UploadListingPhotoValidator
{
    public const long MaxUploadBytes = 10 * 1024 * 1024;
    public const int MaxPhotosPerDraft = 10;

    private static readonly HashSet<string> AllowedMediaTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    public void Validate(UploadListingPhotoCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.DraftId == Guid.Empty)
            throw new ArgumentException("A draft ID is required.", nameof(command));
        ArgumentNullException.ThrowIfNull(command.Content);
        if (command.ContentLength is < 1 or > MaxUploadBytes)
            throw new ArgumentOutOfRangeException(nameof(command), $"Photo uploads must be between 1 byte and {MaxUploadBytes} bytes.");
        if (!AllowedMediaTypes.Contains(command.DeclaredMediaType))
            throw new ArgumentException("Only JPEG, PNG, and WebP photos are accepted.", nameof(command));
    }

    public static string GetMediaTypeForFormat(string detectedFormatName) => detectedFormatName.ToUpperInvariant() switch
    {
        "JPEG" => "image/jpeg",
        "PNG" => "image/png",
        "WEBP" => "image/webp",
        _ => throw new ArgumentException("The detected image format is not supported.", nameof(detectedFormatName))
    };
}
