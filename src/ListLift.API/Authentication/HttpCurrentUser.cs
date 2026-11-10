using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ListLift.Application.Abstractions;

namespace ListLift.API.Authentication;

internal sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string UserId
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException("An authenticated user is required.");

            return principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("The authenticated identity has no user ID claim.");
        }
    }
}
