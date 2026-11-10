using ListLift.Application.ListingDrafts.Commands;
using ListLift.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ListLift.API.Controllers;

[ApiController]
[Authorize]
[Route("api/categories")]
public sealed class MarketplaceCategoriesController(SearchMarketplaceCategoriesHandler searchHandler) : ControllerBase
{
    [HttpGet("{marketplace}/suggestions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        string marketplace,
        [FromQuery] string query,
        CancellationToken cancellationToken)
    {
        var channel = marketplace.ToLowerInvariant() switch
        {
            "ebay" => MarketplaceChannel.Ebay,
            "vinted" => MarketplaceChannel.Vinted,
            _ => throw new System.ComponentModel.DataAnnotations.ValidationException("Choose eBay or Vinted as the marketplace.")
        };

        var suggestions = await searchHandler.HandleAsync(
            new SearchMarketplaceCategoriesQuery(channel, query),
            cancellationToken);
        return Ok(suggestions);
    }
}
