namespace ListLift.Client.Models;

public sealed record DraftRecoveryState(
    CreateDraftFormModel CreateDraft,
    Guid? DraftId,
    UpdateFactsFormModel Facts,
    DescriptionFormModel Description,
    string EbayCategoryQuery);
