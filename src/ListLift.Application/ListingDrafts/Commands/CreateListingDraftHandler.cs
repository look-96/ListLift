using ListLift.Application.Abstractions;
using ListLift.Application.Validation;
using ListLift.Domain.Entities;
using ListLift.Domain.ValueObjects;

namespace ListLift.Application.ListingDrafts.Commands;

public sealed class CreateListingDraftHandler(
    IListingDraftRepository repository,
    ICurrentUser currentUser,
    IClock clock)
{
    public async Task<Guid> HandleAsync(
        CreateListingDraftCommand command,
        CancellationToken cancellationToken)
    {
        DataAnnotationsRequestValidator.Validate(command);

        var draft = ListingDraft.Create(
            currentUser.UserId,
            command.ProductName,
            command.Category,
            Money.Create(command.PriceAmount, command.CurrencyCode),
            clock.UtcNow,
            command.Quantity ?? 1);

        await repository.AddAsync(draft, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return draft.Id;
    }
}
