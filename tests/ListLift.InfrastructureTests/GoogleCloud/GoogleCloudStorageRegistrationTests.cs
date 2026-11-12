using ListLift.Infrastructure.GoogleCloud;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ListLift.InfrastructureTests.GoogleCloud;

public sealed class GoogleCloudStorageRegistrationTests
{
    [Fact]
    public void AddListingStorage_RequiresGoogleCloudProviderOnCloudRun()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["K_SERVICE"] = "listlift-dev"
        });

        var exception = Assert.Throws<InvalidOperationException>(
            () => GoogleCloudStorageRegistration.AddListingStorage(new ServiceCollection(), configuration));

        Assert.Contains("Storage:PhotoProvider", exception.Message);
    }

    [Fact]
    public void AddListingStorage_RejectsLocalDiskOnCloudRun()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["K_SERVICE"] = "listlift-dev",
            ["Storage:PhotoProvider"] = "LocalFile"
        });

        var exception = Assert.Throws<InvalidOperationException>(
            () => GoogleCloudStorageRegistration.AddListingStorage(new ServiceCollection(), configuration));

        Assert.Contains("durable listing photos", exception.Message);
    }

    [Fact]
    public void AddListingStorage_RequiresValidBucketBeforeCreatingGoogleClients()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["Storage:PhotoProvider"] = "GoogleCloudStorage",
            ["Storage:GoogleCloudBucket"] = "INVALID_BUCKET"
        });

        var exception = Assert.Throws<InvalidOperationException>(
            () => GoogleCloudStorageRegistration.AddListingStorage(new ServiceCollection(), configuration));

        Assert.Contains("Google Cloud Storage bucket", exception.Message);
    }

    [Fact]
    public void AddListingStorage_RequiresKmsKeyNameBeforeCreatingGoogleClients()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["Storage:PhotoProvider"] = "GoogleCloudStorage",
            ["Storage:GoogleCloudBucket"] = "listlift-test-bucket"
        });

        var exception = Assert.Throws<InvalidOperationException>(
            () => GoogleCloudStorageRegistration.AddListingStorage(new ServiceCollection(), configuration));

        Assert.Contains("DataProtection:KmsKeyName", exception.Message);
    }

    private static IConfiguration CreateConfiguration(IDictionary<string, string?> settings) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
}
