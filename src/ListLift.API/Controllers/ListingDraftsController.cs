using ListLift.API.Models;
using ListLift.Application.ListingDrafts.Commands;
using ListLift.Application.ListingDrafts.Queries;
using ListLift.Application.ListingDrafts.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ListLift.API.Controllers;

[ApiController]
[Authorize]
[Route("api/drafts")]
public sealed class ListingDraftsController(
    CreateListingDraftHandler createHandler,
    UpdateListingProductFactsHandler updateFactsHandler,
    SaveApprovedDescriptionHandler saveDescriptionHandler,
    GenerateListingDescriptionHandler generateDescriptionHandler,
    GetListingDraftHandler getHandler,
    GetActiveListingDraftsHandler listHandler,
    DeleteListingDraftHandler deleteHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ListingDraftDetails>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ListingDraftDetails>> Create(
        CreateListingDraftRequest request,
        CancellationToken cancellationToken)
    {
        var draftId = await createHandler.HandleAsync(
            new CreateListingDraftCommand(
                request.ProductName,
                request.Category,
                request.PriceAmount,
                request.CurrencyCode,
                request.Quantity),
            cancellationToken);

        var draft = await getHandler.HandleAsync(new GetListingDraftQuery(draftId), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { draftId }, draft);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ListingDraftDetails>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ListingDraftDetails>>> GetActive(
        CancellationToken cancellationToken)
    {
        var drafts = await listHandler.HandleAsync(new GetActiveListingDraftsQuery(), cancellationToken);
        return Ok(drafts);
    }

    [HttpGet("{draftId:guid}")]
    [ProducesResponseType<ListingDraftDetails>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ListingDraftDetails>> GetById(
        Guid draftId,
        CancellationToken cancellationToken)
    {
        var draft = await getHandler.HandleAsync(new GetListingDraftQuery(draftId), cancellationToken);
        return Ok(draft);
    }

    [HttpPut("{draftId:guid}/facts")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateFacts(
        Guid draftId,
        UpdateListingProductFactsRequest request,
        CancellationToken cancellationToken)
    {
        await updateFactsHandler.HandleAsync(
            new UpdateListingProductFactsCommand(
                draftId,
                request.ProductName,
                request.Category,
                request.ConditionDescription,
                request.VerifiedAttributes,
                request.PriceAmount,
                request.CurrencyCode,
                request.Quantity),
            cancellationToken);

        return NoContent();
    }

    [HttpPut("{draftId:guid}/description")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SaveDescription(
        Guid draftId,
        SaveApprovedDescriptionRequest request,
        CancellationToken cancellationToken)
    {
        await saveDescriptionHandler.HandleAsync(
            new SaveApprovedDescriptionCommand(draftId, request.Description),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{draftId:guid}/description/generate")]
    [ProducesResponseType<GeneratedListingDescriptionResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GeneratedListingDescriptionResponse>> GenerateDescription(
        Guid draftId,
        CancellationToken cancellationToken)
    {
        var suggestion = await generateDescriptionHandler.HandleAsync(
            new GenerateListingDescriptionCommand(draftId),
            cancellationToken);

        return Ok(new GeneratedListingDescriptionResponse(suggestion, IsDemoSuggestion: true));
    }

    [HttpDelete("{draftId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid draftId,
        CancellationToken cancellationToken)
    {
        var deleted = await deleteHandler.HandleAsync(
            new DeleteListingDraftCommand(draftId),
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
