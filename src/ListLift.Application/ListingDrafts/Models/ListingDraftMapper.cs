using ListLift.Domain.Entities;

namespace ListLift.Application.ListingDrafts.Models;

internal static class ListingDraftMapper
{
    public static ListingDraftDetails ToDetails(ListingDraft draft) => new(
        draft.Id,
        draft.ProductName,
        draft.Category,
        draft.ConditionDescription,
        draft.VerifiedAttributes,
        draft.Description,
        draft.Price.Amount,
        draft.Price.CurrencyCode,
        draft.Quantity,
        draft.Status,
        draft.CreatedAtUtc,
        draft.ExpiresAtUtc,
        draft.Photos
            .OrderBy(photo => photo.SortOrder)
            .Select(photo => new ListingPhotoDetails(
                photo.Id,
                photo.MediaType,
                photo.SizeBytes,
                photo.SortOrder,
                photo.CreatedAtUtc))
            .ToArray(),
        draft.ChannelListings
            .OrderBy(listing => listing.Channel)
            .Select(listing => new ChannelListingDetails(
                listing.Id,
                listing.Channel,
                listing.Status,
                listing.ExternalListingId,
                listing.SafeErrorCode,
                listing.UpdatedAtUtc))
            .ToArray(),
        draft.CategorySelections
            .OrderBy(selection => selection.Channel)
            .Select(selection => new ListingCategorySelectionDetails(
                selection.Channel,
                selection.CategoryId,
                selection.CategoryName,
                selection.CategoryPath,
                selection.SelectedAtUtc))
            .ToArray());
}
