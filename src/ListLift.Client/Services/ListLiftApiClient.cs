using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using ListLift.Client.Models;

namespace ListLift.Client.Services;

public sealed class ListLiftApiClient(HttpClient http, AuthSession session)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task RegisterAsync(AuthFormModel form, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("auth/register", form, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task LoginAsync(AuthFormModel form, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("auth/login?useCookies=false", form, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var tokens = await response.Content.ReadFromJsonAsync<IdentityTokenResponse>(JsonOptions, cancellationToken)
            ?? throw new ApiClientException("The sign-in response was empty.");
        session.SetTokens(tokens.AccessToken, tokens.RefreshToken, form.Email);
    }

    public async Task<IReadOnlyList<ListingDraftDetailsDto>> GetActiveDraftsAsync(CancellationToken cancellationToken = default) =>
        await SendAsync<IReadOnlyList<ListingDraftDetailsDto>>(HttpMethod.Get, "api/drafts", null, cancellationToken)
        ?? [];

    public Task<ListingDraftDetailsDto?> GetDraftAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync<ListingDraftDetailsDto>(HttpMethod.Get, $"api/drafts/{id}", null, cancellationToken);

    public Task<ListingDraftDetailsDto?> CreateDraftAsync(CreateDraftFormModel form, CancellationToken cancellationToken = default) =>
        SendAsync<ListingDraftDetailsDto>(HttpMethod.Post, "api/drafts", form, cancellationToken);

    public Task UpdateFactsAsync(Guid id, UpdateFactsFormModel form, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Put, $"api/drafts/{id}/facts", form, cancellationToken);

    public Task SaveDescriptionAsync(Guid id, DescriptionFormModel form, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Put, $"api/drafts/{id}/description", form, cancellationToken);

    public Task<GeneratedListingDescriptionDto?> GenerateDescriptionAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync<GeneratedListingDescriptionDto>(HttpMethod.Post, $"api/drafts/{id}/description/generate", null, cancellationToken);

    public async Task<IReadOnlyList<MarketplaceCategorySuggestionDto>> SearchMarketplaceCategoriesAsync(
        string marketplace,
        string query,
        CancellationToken cancellationToken = default) =>
        await SendAsync<IReadOnlyList<MarketplaceCategorySuggestionDto>>(
            HttpMethod.Get,
            $"api/categories/{Uri.EscapeDataString(marketplace)}/suggestions?query={Uri.EscapeDataString(query)}",
            null,
            cancellationToken) ?? [];

    public Task SelectMarketplaceCategoryAsync(
        Guid draftId,
        string marketplace,
        MarketplaceCategorySuggestionDto category,
        CancellationToken cancellationToken = default) =>
        SendNoContentAsync(
            HttpMethod.Put,
            $"api/drafts/{draftId}/categories/{Uri.EscapeDataString(marketplace)}",
            new { category.CategoryId, CategoryName = category.Name },
            cancellationToken);

    public Task DeleteDraftAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Delete, $"api/drafts/{id}", null, cancellationToken);

    public async Task UploadPhotoAsync(Guid id, IBrowserFile file, CancellationToken cancellationToken = default)
    {
        using var response = await SendRequestAsync(() =>
        {
            var content = new MultipartFormDataContent();
            var photo = new StreamContent(file.OpenReadStream(10 * 1024 * 1024, cancellationToken));
            photo.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(photo, "Photo", file.Name);
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/drafts/{id}/photos") { Content = content };
            if (session.AccessToken is { Length: > 0 } token)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public void SignOut() => session.Clear();

    private async Task<T?> SendAsync<T>(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var response = await SendRequestAsync(() => CreateRequest(method, path, body), cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    private async Task SendNoContentAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var response = await SendRequestAsync(() => CreateRequest(method, path, body), cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, object? body)
    {
        var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = JsonContent.Create(body, options: JsonOptions);
        if (session.AccessToken is { Length: > 0 } token)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private async Task<HttpResponseMessage> SendRequestAsync(Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken)
    {
        using var request = requestFactory();
        var response = await http.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized || string.IsNullOrWhiteSpace(session.RefreshToken))
            return response;

        response.Dispose();
        try
        {
            await RefreshAsync(cancellationToken);
            using var retryRequest = requestFactory();
            return await http.SendAsync(retryRequest, cancellationToken);
        }
        catch
        {
            session.Clear();
            throw;
        }
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        using var response = await http.PostAsJsonAsync("auth/refresh", new { session.RefreshToken }, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var tokens = await response.Content.ReadFromJsonAsync<IdentityTokenResponse>(JsonOptions, cancellationToken)
            ?? throw new ApiClientException("The refresh response was empty.");
        session.SetTokens(tokens.AccessToken, tokens.RefreshToken, session.AccountEmail);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;
        var message = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Your session has expired. Please sign in again.",
            HttpStatusCode.Forbidden => "You are not allowed to perform this action.",
            HttpStatusCode.NotFound => "This draft could not be found or has expired.",
            HttpStatusCode.TooManyRequests => "Too many requests. Please wait a moment and try again.",
            HttpStatusCode.ServiceUnavailable => "eBay category search is unavailable until marketplace access is configured.",
            _ => "The request could not be completed. Check the submitted details and try again."
        };

        try
        {
            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            if (document.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var first = errors.EnumerateObject()
                    .Where(p => p.Value.ValueKind == JsonValueKind.Array)
                    .SelectMany(p => p.Value.EnumerateArray())
                    .FirstOrDefault();
                if (first.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(first.GetString()))
                    message = first.GetString()!;
            }
        }
        catch (JsonException) { }

        throw new ApiClientException(message);
    }
}
