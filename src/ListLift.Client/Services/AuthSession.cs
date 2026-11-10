namespace ListLift.Client.Services;

public sealed class AuthSession
{
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public string? AccountEmail { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);

    public event Action? Changed;

    public void SetTokens(string accessToken, string refreshToken, string? accountEmail = null)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        if (!string.IsNullOrWhiteSpace(accountEmail)) AccountEmail = accountEmail.Trim();
        Changed?.Invoke();
    }

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
        AccountEmail = null;
        Changed?.Invoke();
    }
}
