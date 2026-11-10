using System.Text;
using ListLift.Infrastructure.Photos;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace ListLift.InfrastructureTests.Photos;

public sealed class PrivateFileListingPhotoStorageTests
{
    [Fact]
    public async Task StoreAsync_CreatesOpaqueFileOutsideWebRootAndDeleteRemovesIt()
    {
        var contentRoot = CreateTemporaryRoot();
        try
        {
            var storage = new PrivateFileListingPhotoStorage(new TestHostEnvironment(contentRoot));
            var bytes = Encoding.UTF8.GetBytes("normalized-image-bytes");

            var key = await storage.StoreAsync(bytes, "image/jpeg", CancellationToken.None);
            var filePath = Path.Combine(contentRoot, "App_Data", "ListingPhotos", key);

            Assert.Matches("^[a-f0-9]{32}\\.jpg$", key);
            Assert.True(File.Exists(filePath));
            Assert.Equal(bytes, await File.ReadAllBytesAsync(filePath));

            await storage.DeleteAsync(key, CancellationToken.None);

            Assert.False(File.Exists(filePath));
        }
        finally
        {
            DeleteTemporaryRoot(contentRoot);
        }
    }

    [Fact]
    public async Task DeleteAsync_RejectsPathTraversalKey()
    {
        var contentRoot = CreateTemporaryRoot();
        try
        {
            var storage = new PrivateFileListingPhotoStorage(new TestHostEnvironment(contentRoot));

            await Assert.ThrowsAsync<ArgumentException>(
                () => storage.DeleteAsync("..\\..\\outside.jpg", CancellationToken.None));
        }
        finally
        {
            DeleteTemporaryRoot(contentRoot);
        }
    }

    [Fact]
    public async Task StoreAsync_RejectsUnsupportedMediaType()
    {
        var contentRoot = CreateTemporaryRoot();
        try
        {
            var storage = new PrivateFileListingPhotoStorage(new TestHostEnvironment(contentRoot));

            await Assert.ThrowsAsync<ArgumentException>(
                () => storage.StoreAsync(new byte[] { 1, 2, 3 }, "image/gif", CancellationToken.None));
        }
        finally
        {
            DeleteTemporaryRoot(contentRoot);
        }
    }

    private static string CreateTemporaryRoot()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", $"ListLiftInfrastructureTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteTemporaryRoot(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "ListLift.InfrastructureTests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
