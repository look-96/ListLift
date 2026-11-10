using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ListLift.Application.Abstractions;
using ListLift.Application.ListingDrafts.Exceptions;
using ListLift.Domain.Constants;

namespace ListLift.Infrastructure.Marketplaces.Ebay;

internal sealed class EbayTaxonomyCategoryProvider(
    IHttpClientFactory httpClientFactory,
    IEbayApplicationSettingsStore settingsStore) : IMarketplaceCategoryProvider
{
    private const string ApiRoot = "https://api.ebay.com/commerce/taxonomy/v1";
    private const string TokenEndpoint = "https://api.ebay.com/identity/v1/oauth2/token";
    private const string MarketplaceId = "EBAY_GB";
    private const string TaxonomyScope = "https://api.ebay.com/oauth/api_scope";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private readonly SemaphoreSlim _treeLock = new(1, 1);
    private string? _accessToken;
    private DateTimeOffset _tokenExpiresAtUtc;
    private string? _categoryTreeId;

    public MarketplaceChannel Channel => MarketplaceChannel.Ebay;

    public async Task<IReadOnlyList<MarketplaceCategorySuggestion>> SearchAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var credentials = await settingsStore.GetAsync(cancellationToken);
        if (credentials is null)
            throw new MarketplaceCategoryProviderUnavailableException();

        var client = httpClientFactory.CreateClient("EbayTaxonomy");
        var token = await GetAccessTokenAsync(client, credentials, cancellationToken);
        var treeId = await GetCategoryTreeIdAsync(client, token, cancellationToken);
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"{ApiRoot}/category_tree/{Uri.EscapeDataString(treeId)}/get_category_suggestions?q={Uri.EscapeDataString(query)}",
            token);
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<CategorySuggestionResponse>(JsonOptions, cancellationToken)
            ?? throw new HttpRequestException("eBay returned an empty category suggestion response.");

        return payload.CategorySuggestions
            .Where(item => !string.IsNullOrWhiteSpace(item.Category.CategoryId)
                && !string.IsNullOrWhiteSpace(item.Category.CategoryName))
            .Select(item =>
            {
                var pathSegments = item.CategoryTreeNodeAncestors
                    .OrderBy(ancestor => ancestor.CategoryTreeNodeLevel)
                    .Select(ancestor => ancestor.CategoryName)
                    .Append(item.Category.CategoryName)
                    .Where(name => !string.IsNullOrWhiteSpace(name));
                return new MarketplaceCategorySuggestion(
                    MarketplaceChannel.Ebay,
                    item.Category.CategoryId,
                    item.Category.CategoryName,
                    string.Join(" / ", pathSegments));
            })
            .Take(10)
            .ToArray();
    }

    private async Task<string> GetAccessTokenAsync(
        HttpClient client,
        EbayApplicationCredentials credentials,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_accessToken) && DateTimeOffset.UtcNow < _tokenExpiresAtUtc)
            return _accessToken;

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(_accessToken) && DateTimeOffset.UtcNow < _tokenExpiresAtUtc)
                return _accessToken;

            var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.ClientId}:{credentials.ClientSecret}"));
            using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["scope"] = TaxonomyScope
            });

            using var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var tokenPayload = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions, cancellationToken)
                ?? throw new HttpRequestException("eBay returned an empty OAuth token response.");
            if (string.IsNullOrWhiteSpace(tokenPayload.AccessToken) || tokenPayload.ExpiresIn <= 60)
                throw new HttpRequestException("eBay returned an invalid OAuth token response.");

            _accessToken = tokenPayload.AccessToken;
            _tokenExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(tokenPayload.ExpiresIn - 60);
            return _accessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task<string> GetCategoryTreeIdAsync(
        HttpClient client,
        string token,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_categoryTreeId))
            return _categoryTreeId;

        await _treeLock.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(_categoryTreeId))
                return _categoryTreeId;

            using var request = CreateAuthorizedRequest(
                HttpMethod.Get,
                $"{ApiRoot}/get_default_category_tree_id?marketplace_id={MarketplaceId}",
                token);
            using var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<CategoryTreeResponse>(JsonOptions, cancellationToken)
                ?? throw new HttpRequestException("eBay returned an empty category tree response.");
            if (string.IsNullOrWhiteSpace(payload.CategoryTreeId))
                throw new HttpRequestException("eBay did not return a UK category tree ID.");

            _categoryTreeId = payload.CategoryTreeId;
            return _categoryTreeId;
        }
        finally
        {
            _treeLock.Release();
        }
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string uri, string token)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-EBAY-C-MARKETPLACE-ID", MarketplaceId);
        request.Headers.AcceptLanguage.ParseAdd("en-GB");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    private sealed record CategoryTreeResponse(
        [property: JsonPropertyName("categoryTreeId")] string CategoryTreeId);

    private sealed record CategorySuggestionResponse(
        [property: JsonPropertyName("categorySuggestions")] IReadOnlyList<CategorySuggestion> CategorySuggestions);

    private sealed record CategorySuggestion(
        [property: JsonPropertyName("category")] Category Category,
        [property: JsonPropertyName("categoryTreeNodeAncestors")] IReadOnlyList<CategoryAncestor> CategoryTreeNodeAncestors);

    private sealed record Category(
        [property: JsonPropertyName("categoryId")] string CategoryId,
        [property: JsonPropertyName("categoryName")] string CategoryName);

    private sealed record CategoryAncestor(
        [property: JsonPropertyName("categoryName")] string CategoryName,
        [property: JsonPropertyName("categoryTreeNodeLevel")] int CategoryTreeNodeLevel);
}
