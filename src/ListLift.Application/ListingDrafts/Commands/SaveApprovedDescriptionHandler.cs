using ListLift.Application.Abstractions;
using ListLift.Application.ListingDrafts.Exceptions;
using ListLift.Application.Validation;

namespace ListLift.Application.ListingDrafts.Commands;

public sealed class SaveApprovedDescriptionHandler(
    IListingDraftRepository repository,
    ICurrentUser currentUser,
    IClock clock)
{
    public async Task HandleAsync(
        SaveApprovedDescriptionCommand command,
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
            throw new ListingDraftNotFoundException(command.DraftId);

        draft.SaveApprovedDescription(command.Description, clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
