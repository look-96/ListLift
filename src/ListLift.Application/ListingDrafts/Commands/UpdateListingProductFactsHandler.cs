using ListLift.Application.Abstractions;
using ListLift.Application.ListingDrafts.Exceptions;
using ListLift.Application.Validation;
using ListLift.Domain.ValueObjects;

namespace ListLift.Application.ListingDrafts.Commands;

public sealed class UpdateListingProductFactsHandler(
    IListingDraftRepository repository,
    ICurrentUser currentUser,
    IClock clock)
{
    public async Task HandleAsync(
        UpdateListingProductFactsCommand command,
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

        draft.UpdateProductFacts(
            command.ProductName,
            command.Category,
            command.ConditionDescription,
            command.VerifiedAttributes,
            Money.Create(command.PriceAmount, command.CurrencyCode),
            clock.UtcNow,
            command.Quantity);

        await repository.SaveChangesAsync(cancellationToken);
    }
}
