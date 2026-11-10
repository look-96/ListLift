namespace ListLift.Client.Models;

public sealed record IdentityTokenResponse(
    string TokenType,
    string AccessToken,
    int ExpiresIn,
    string RefreshToken);
