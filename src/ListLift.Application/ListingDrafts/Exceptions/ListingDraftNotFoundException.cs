namespace ListLift.Application.ListingDrafts.Exceptions;

public sealed class ListingDraftNotFoundException(Guid draftId)
    : Exception($"Listing draft '{draftId}' was not found.");
