using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ListLift.Client.Models;
using Microsoft.JSInterop;

namespace ListLift.Client.Services;

public sealed class DraftRecoveryStore(IJSRuntime js)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<DraftRecoveryState?> LoadAsync(string? accountEmail)
    {
        if (string.IsNullOrWhiteSpace(accountEmail)) return null;

        try
        {
            var json = await js.InvokeAsync<string?>("listLiftRecovery.load", GetStorageKey(accountEmail));
            return string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<DraftRecoveryState>(json, JsonOptions);
        }
        catch (Exception exception) when (exception is JSException or JsonException)
        {
            return null;
        }
    }

    public async Task<bool> SaveAsync(string? accountEmail, DraftRecoveryState state)
    {
        if (string.IsNullOrWhiteSpace(accountEmail)) return false;

        try
        {
            var json = JsonSerializer.Serialize(state, JsonOptions);
            await js.InvokeVoidAsync("listLiftRecovery.save", GetStorageKey(accountEmail), json);
            return true;
        }
        catch (JSException)
        {
            return false;
        }
    }

    public async Task ClearAsync(string? accountEmail)
    {
        if (string.IsNullOrWhiteSpace(accountEmail)) return;

        try
        {
            await js.InvokeVoidAsync("listLiftRecovery.clear", GetStorageKey(accountEmail));
        }
        catch (JSException)
        {
            // Storage may be unavailable or blocked by the browser.
        }
    }

    private static string GetStorageKey(string email)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedEmail));
        return $"listlift.recovery.v1.{Convert.ToHexString(digest).ToLowerInvariant()}";
    }
}
