using ListLift.Application.Abstractions;
using ListLift.Application.ListingDrafts.Exceptions;
using ListLift.Application.Validation;
using ListLift.Domain.Entities;

namespace ListLift.Application.ListingDrafts.Commands;

public sealed class SelectMarketplaceCategoryHandler(
    IListingDraftRepository repository,
    ICurrentUser currentUser,
    IEnumerable<IMarketplaceCategoryProvider> providers,
    IClock clock)
{
    public async Task HandleAsync(SelectMarketplaceCategoryCommand command, CancellationToken cancellationToken)
    {
        DataAnnotationsRequestValidator.Validate(command);
        if (command.DraftId == Guid.Empty)
            throw new ArgumentException("A draft ID is required.", nameof(command));

        var draft = await repository.GetOwnedAsync(command.DraftId, currentUser.UserId, cancellationToken);
        if (draft is null)
            throw new ListingDraftNotFoundException(command.DraftId);

        var provider = providers.SingleOrDefault(candidate => candidate.Channel == command.Channel)
            ?? throw new MarketplaceCategoryProviderUnavailableException();
        var candidates = await provider.SearchAsync(command.CategoryName, cancellationToken);
        var selectedCategory = candidates.SingleOrDefault(category => category.CategoryId == command.CategoryId)
            ?? throw new System.ComponentModel.DataAnnotations.ValidationException("Choose a category from the current marketplace suggestions.");

        var selection = ListingCategorySelection.Create(
            draft.Id,
            command.Channel,
            selectedCategory.CategoryId,
            selectedCategory.Name,
            selectedCategory.Path,
            clock.UtcNow);

        draft.SetMarketplaceCategory(selection, clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
