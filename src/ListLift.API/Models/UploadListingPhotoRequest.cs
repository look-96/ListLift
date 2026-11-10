using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ListLift.API.Models;

public sealed class UploadListingPhotoRequest
{
    [Required]
    public IFormFile? Photo { get; init; }
}
