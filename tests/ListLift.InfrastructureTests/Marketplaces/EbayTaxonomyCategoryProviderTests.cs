using ListLift.Application.Abstractions;
using ListLift.Application.ListingDrafts.Exceptions;
using ListLift.Infrastructure.Marketplaces.Ebay;

namespace ListLift.InfrastructureTests.Marketplaces;

public sealed class EbayTaxonomyCategoryProviderTests
{
    [Fact]
    public async Task SearchAsync_WhenCredentialsAreNotConfigured_DoesNotCreateHttpClient()
    {
        var factory = new UnexpectedHttpClientFactory();
        var provider = new EbayTaxonomyCategoryProvider(factory, new EmptyEbaySettingsStore());

        await Assert.ThrowsAsync<MarketplaceCategoryProviderUnavailableException>(
            () => provider.SearchAsync("camera", CancellationToken.None));

        Assert.False(factory.WasCalled);
    }

    private sealed class UnexpectedHttpClientFactory : IHttpClientFactory
    {
        public bool WasCalled { get; private set; }

        public HttpClient CreateClient(string name)
        {
            WasCalled = true;
            throw new InvalidOperationException("No external HTTP request should occur without settings.");
        }
    }

    private sealed class EmptyEbaySettingsStore : IEbayApplicationSettingsStore
    {
        public Task<EbayApplicationCredentials?> GetAsync(CancellationToken cancellationToken) => Task.FromResult<EbayApplicationCredentials?>(null);
        public Task SetAsync(EbayApplicationCredentials credentials, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task DeleteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
