using ListLift.Application.Abstractions;
using ListLift.Application.ListingDrafts.Exceptions;

namespace ListLift.Application.ListingDrafts.Commands;

public sealed class GenerateListingDescriptionHandler(
    IListingDraftRepository repository,
    ICurrentUser currentUser,
    IProductDescriptionGenerator generator,
    IClock clock)
{
    public async Task<string> HandleAsync(
        GenerateListingDescriptionCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.DraftId == Guid.Empty)
            throw new ArgumentException("A draft ID is required.", nameof(command));

        var draft = await repository.GetOwnedAsync(command.DraftId, currentUser.UserId, cancellationToken);
        if (draft is null)
            throw new ListingDraftNotFoundException(command.DraftId);

        draft.EnsureActive(clock.UtcNow);
        var generated = await generator.GenerateAsync(
            new ProductDescriptionInput(
                draft.ProductName,
                draft.Category,
                draft.ConditionDescription,
                draft.VerifiedAttributes,
                draft.Price.Amount,
                draft.Price.CurrencyCode),
            cancellationToken);

        draft.SetDescriptionSuggestion(generated, clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return draft.Description!;
    }
}
