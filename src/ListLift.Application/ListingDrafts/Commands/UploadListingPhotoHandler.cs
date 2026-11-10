using ListLift.Application.Abstractions;
using ListLift.Application.ListingDrafts.Exceptions;
using ListLift.Domain.Entities;

namespace ListLift.Application.ListingDrafts.Commands;

public sealed class UploadListingPhotoHandler(
    IListingDraftRepository repository,
    ICurrentUser currentUser,
    IListingPhotoProcessor processor,
    IListingPhotoStorage storage,
    IClock clock,
    UploadListingPhotoValidator validator)
{
    public async Task<Guid> HandleAsync(
        UploadListingPhotoCommand command,
        CancellationToken cancellationToken)
    {
        validator.Validate(command);

        var draft = await repository.GetOwnedAsync(
            command.DraftId,
            currentUser.UserId,
            cancellationToken);

        if (draft is null)
            throw new ListingDraftNotFoundException(command.DraftId);
        if (draft.Photos.Count >= UploadListingPhotoValidator.MaxPhotosPerDraft)
            throw new InvalidOperationException("The draft has reached its photo limit.");

        var processedPhoto = await processor.ProcessAsync(command.Content, cancellationToken);
        if (!string.Equals(processedPhoto.MediaType, command.DeclaredMediaType, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The declared content type does not match the image content.", nameof(command));

        var storageKey = await storage.StoreAsync(
            processedPhoto.Content,
            processedPhoto.MediaType,
            cancellationToken);

        try
        {
            var photo = ListingPhoto.Create(
                draft.Id,
                storageKey,
                processedPhoto.MediaType,
                processedPhoto.Content.LongLength,
                draft.Photos.Count,
                clock.UtcNow);
            draft.AddPhoto(photo, clock.UtcNow);
            await repository.SaveChangesAsync(cancellationToken);
            return photo.Id;
        }
        catch
        {
            try
            {
                await storage.DeleteAsync(storageKey, CancellationToken.None);
            }
            catch
            {
            }

            throw;
        }
    }
}
