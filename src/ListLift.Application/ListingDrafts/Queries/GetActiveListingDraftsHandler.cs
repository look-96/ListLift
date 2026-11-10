using ListLift.Application.Abstractions;
using ListLift.Application.ListingDrafts.Models;
using ListLift.Application.Validation;

namespace ListLift.Application.ListingDrafts.Queries;

public sealed class GetActiveListingDraftsHandler(
    IListingDraftRepository repository,
    ICurrentUser currentUser,
    IClock clock)
{
    public async Task<IReadOnlyList<ListingDraftDetails>> HandleAsync(
        GetActiveListingDraftsQuery query,
        CancellationToken cancellationToken)
    {
        DataAnnotationsRequestValidator.Validate(query);

        var drafts = await repository.GetActiveForOwnerAsync(
            currentUser.UserId,
            clock.UtcNow,
            cancellationToken);

        return drafts.Select(ListingDraftMapper.ToDetails).ToArray();
    }
}
