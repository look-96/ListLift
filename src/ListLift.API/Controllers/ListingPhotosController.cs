using ListLift.API.Models;
using ListLift.Application.ListingDrafts.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ListLift.API.Controllers;

[ApiController]
[Authorize]
[Route("api/drafts/{draftId:guid}/photos")]
public sealed class ListingPhotosController(UploadListingPhotoHandler uploadHandler) : ControllerBase
{
    private const int RequestOverheadBytes = 256 * 1024;
    private const long MaxRequestBytes = UploadListingPhotoValidator.MaxUploadBytes + RequestOverheadBytes;

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Upload(
        Guid draftId,
        [FromForm] UploadListingPhotoRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Photo is null)
        {
            var errors = new Dictionary<string, string[]> { ["photo"] = ["A photo is required."] };
            return BadRequest(new ValidationProblemDetails(errors));
        }

        await using var content = request.Photo.OpenReadStream();
        var photoId = await uploadHandler.HandleAsync(
            new UploadListingPhotoCommand(
                draftId,
                content,
                request.Photo.Length,
                request.Photo.ContentType),
            cancellationToken);

        return Ok(new { photoId });
    }
}
