using System.ComponentModel.DataAnnotations;

namespace ListLift.Application.ListingDrafts.Queries;

public sealed record GetListingDraftQuery(
    Guid DraftId);
