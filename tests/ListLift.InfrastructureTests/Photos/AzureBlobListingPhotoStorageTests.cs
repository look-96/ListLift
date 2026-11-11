using Azure.Identity;
using Azure.Storage.Blobs;
using ListLift.Infrastructure.Photos;

namespace ListLift.InfrastructureTests.Photos;

public sealed class AzureBlobListingPhotoStorageTests
{
    [Fact]
    public async Task StoreAsync_RejectsEmptyPhotoBeforeContactingAzure()
    {
        var storage = CreateStorage();

        await Assert.ThrowsAsync<ArgumentException>(
            () => storage.StoreAsync(ReadOnlyMemory<byte>.Empty, "image/jpeg", CancellationToken.None));
    }

    [Fact]
    public async Task StoreAsync_RejectsUnsupportedMediaTypeBeforeContactingAzure()
    {
        var storage = CreateStorage();

        await Assert.ThrowsAsync<ArgumentException>(
            () => storage.StoreAsync(new byte[] { 1, 2, 3 }, "image/svg+xml", CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_RejectsPathTraversalKeyBeforeContactingAzure()
    {
        var storage = CreateStorage();

        await Assert.ThrowsAsync<ArgumentException>(
            () => storage.DeleteAsync("../outside.jpg", CancellationToken.None));
    }

    private static AzureBlobListingPhotoStorage CreateStorage()
    {
        var container = new BlobContainerClient(
            new Uri("https://listlift-test.blob.core.windows.net/listing-photos"),
            new DefaultAzureCredential());
        return new AzureBlobListingPhotoStorage(container);
    }
}
