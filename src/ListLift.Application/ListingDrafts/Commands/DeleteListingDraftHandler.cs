using ListLift.Application.Abstractions;
using ListLift.Application.Validation;
using Microsoft.Extensions.Logging;

namespace ListLift.Application.ListingDrafts.Commands;

public sealed class DeleteListingDraftHandler(
    IListingDraftRepository repository,
    ICurrentUser currentUser,
    IListingPhotoStorage photoStorage,
    ILogger<DeleteListingDraftHandler> logger)
{
    public async Task<bool> HandleAsync(
        DeleteListingDraftCommand command,
        CancellationToken cancellationToken)
    {
        DataAnnotationsRequestValidator.Validate(command);
        if (command.DraftId == Guid.Empty)
            throw new ArgumentException("A draft ID is required.", nameof(command));

        var draft = await repository.GetOwnedAsync(
            command.DraftId,
            currentUser.UserId,
            cancellationToken);

        if (draft is null)
            return false;

        var storageKeys = draft.Photos.Select(photo => photo.StorageKey).ToArray();
        repository.DeleteRange([draft]);
        await repository.SaveChangesAsync(cancellationToken);

        foreach (var storageKey in storageKeys)
        {
            try
            {
                await photoStorage.DeleteAsync(storageKey, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Could not remove private photo {StorageKey} after deleting its draft.", storageKey);
            }
        }

        return true;
    }
}
