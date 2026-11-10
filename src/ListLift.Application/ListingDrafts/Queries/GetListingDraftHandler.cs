using ListLift.Application.Abstractions;
using ListLift.Application.ListingDrafts.Exceptions;
using ListLift.Application.ListingDrafts.Models;
using ListLift.Application.Validation;

namespace ListLift.Application.ListingDrafts.Queries;

public sealed class GetListingDraftHandler(
    IListingDraftRepository repository,
    ICurrentUser currentUser)
{
    public async Task<ListingDraftDetails> HandleAsync(
        GetListingDraftQuery query,
        CancellationToken cancellationToken)
    {
        DataAnnotationsRequestValidator.Validate(query);
        if (query.DraftId == Guid.Empty)
            throw new ArgumentException("A draft ID is required.", nameof(query));

        var draft = await repository.GetOwnedAsync(
            query.DraftId,
            currentUser.UserId,
            cancellationToken);

        if (draft is null)
            throw new ListingDraftNotFoundException(query.DraftId);

        return ListingDraftMapper.ToDetails(draft);
    }
}
