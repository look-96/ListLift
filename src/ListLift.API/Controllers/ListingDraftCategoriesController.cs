using ListLift.API.Models;
using ListLift.Application.ListingDrafts.Commands;
using ListLift.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ListLift.API.Controllers;

[ApiController]
[Authorize]
[Route("api/drafts/{draftId:guid}/categories")]
public sealed class ListingDraftCategoriesController(SelectMarketplaceCategoryHandler selectHandler) : ControllerBase
{
    [HttpPut("{marketplace}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Select(
        Guid draftId,
        string marketplace,
        SelectMarketplaceCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var channel = marketplace.ToLowerInvariant() switch
        {
            "ebay" => MarketplaceChannel.Ebay,
            "vinted" => MarketplaceChannel.Vinted,
            _ => throw new System.ComponentModel.DataAnnotations.ValidationException("Choose eBay or Vinted as the marketplace.")
        };

        await selectHandler.HandleAsync(
            new SelectMarketplaceCategoryCommand(
                draftId,
                channel,
                request.CategoryId,
                request.CategoryName),
            cancellationToken);
        return NoContent();
    }
}
