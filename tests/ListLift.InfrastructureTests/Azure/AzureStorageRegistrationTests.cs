using ListLift.Infrastructure.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ListLift.InfrastructureTests.Azure;

public sealed class AzureStorageRegistrationTests
{
    [Fact]
    public void AddListingStorage_RejectsMissingProviderInAppService()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["WEBSITE_SITE_NAME"] = "listlift-api-dev"
        });

        var exception = Assert.Throws<InvalidOperationException>(
            () => AzureStorageRegistration.AddListingStorage(new ServiceCollection(), configuration));

        Assert.Contains("Storage:PhotoProvider", exception.Message);
    }

    [Fact]
    public void AddListingStorage_RejectsLocalDiskProviderInAppService()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["WEBSITE_SITE_NAME"] = "listlift-api-dev",
            ["Storage:PhotoProvider"] = "LocalFile"
        });

        var exception = Assert.Throws<InvalidOperationException>(
            () => AzureStorageRegistration.AddListingStorage(new ServiceCollection(), configuration));

        Assert.Contains("local disk storage is not durable", exception.Message);
    }

    [Fact]
    public void AddListingStorage_RequiresBlobAndKeyProtectionSettingsForAzureProvider()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["Storage:PhotoProvider"] = "AzureBlob",
            ["WEBSITE_SITE_NAME"] = "listlift-api-dev"
        });

        var exception = Assert.Throws<InvalidOperationException>(
            () => AzureStorageRegistration.AddListingStorage(new ServiceCollection(), configuration));

        Assert.Contains("Storage:BlobServiceUri", exception.Message);
    }

    private static IConfiguration CreateConfiguration(IDictionary<string, string?> settings) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
}
