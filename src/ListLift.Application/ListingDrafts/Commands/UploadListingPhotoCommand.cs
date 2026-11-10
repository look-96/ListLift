namespace ListLift.Application.ListingDrafts.Commands;

public sealed record UploadListingPhotoCommand(
    Guid DraftId,
    Stream Content,
    long ContentLength,
    string DeclaredMediaType);
