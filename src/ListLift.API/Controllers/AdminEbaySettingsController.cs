using ListLift.API.Models;
using ListLift.API.Security;
using ListLift.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ListLift.API.Controllers;

[ApiController]
[Authorize]
[EnableRateLimiting("Admin")]
[Route("api/admin/ebay-settings")]
public sealed class AdminEbaySettingsController(
    IEbayApplicationSettingsStore settingsStore,
    IAdminCredentialVerifier credentialVerifier) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!HasAdminCredential())
            return Forbid();

        var credentials = await settingsStore.GetAsync(cancellationToken);
        return Ok(new { isConfigured = credentials is not null });
    }

    [HttpPut]
    public async Task<IActionResult> Set(
        [FromBody] SetEbaySettingsRequest request,
        CancellationToken cancellationToken)
    {
        if (!HasAdminCredential())
            return Forbid();

        await settingsStore.SetAsync(
            new EbayApplicationCredentials(request.ClientId.Trim(), request.ClientSecret.Trim()),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken)
    {
        if (!HasAdminCredential())
            return Forbid();

        await settingsStore.DeleteAsync(cancellationToken);
        return NoContent();
    }

    private bool HasAdminCredential() =>
        credentialVerifier.IsValid(Request.Headers["X-ListLift-Admin-Key"].ToString());
}
