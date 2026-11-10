using System.ComponentModel.DataAnnotations;

namespace ListLift.Application.ListingDrafts.Commands;

public sealed record SaveApprovedDescriptionCommand(
    Guid DraftId,
    [property: Required, StringLength(4000, MinimumLength = 1)] string Description);
