using Azure.Identity;
using Azure.Storage.Blobs;
using ListLift.Application.Abstractions;
using ListLift.Infrastructure.Photos;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ListLift.Infrastructure.Azure;

internal static class AzureStorageRegistration
{
    public static void AddListingStorage(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var photoProvider = configuration["Storage:PhotoProvider"];
        var isAzureAppService = !string.IsNullOrWhiteSpace(configuration["WEBSITE_SITE_NAME"]) ||
                                !string.IsNullOrWhiteSpace(configuration["WEBSITE_INSTANCE_ID"]);

        if (string.IsNullOrWhiteSpace(photoProvider))
        {
            if (isAzureAppService)
                throw new InvalidOperationException("Storage:PhotoProvider must be set to AzureBlob when hosted in Azure App Service.");

            photoProvider = "LocalFile";
        }

        if (isAzureAppService && !string.Equals(photoProvider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Azure App Service must use Azure Blob photo storage; local disk storage is not durable.");

        if (string.Equals(photoProvider, "LocalFile", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IListingPhotoStorage, PrivateFileListingPhotoStorage>();
            services.AddDataProtection().SetApplicationName("ListLift");
            return;
        }

        if (!string.Equals(photoProvider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Storage:PhotoProvider must be LocalFile or AzureBlob.");

        var blobServiceUri = ReadHttpsUri(configuration, "Storage:BlobServiceUri", requireRoot: true);
        var photoContainer = configuration["Storage:PhotoContainer"];
        if (!IsValidContainerName(photoContainer))
            throw new InvalidOperationException("Storage:PhotoContainer must be a valid Azure Blob container name.");

        var keyBlobUri = ReadHttpsUri(configuration, "DataProtection:BlobUri", requireRoot: false);
        var keyVaultKeyIdentifier = ReadHttpsUri(configuration, "DataProtection:KeyVaultKeyIdentifier", requireRoot: false);
        if (!keyVaultKeyIdentifier.AbsolutePath.StartsWith("/keys/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("DataProtection:KeyVaultKeyIdentifier must identify a Key Vault key.");

        var credential = new DefaultAzureCredential();
        var blobServiceClient = new BlobServiceClient(blobServiceUri, credential);
        services.AddSingleton(blobServiceClient);
        services.AddSingleton(sp => sp.GetRequiredService<BlobServiceClient>().GetBlobContainerClient(photoContainer));
        services.AddSingleton<IListingPhotoStorage, AzureBlobListingPhotoStorage>();
        services.AddDataProtection()
            .SetApplicationName("ListLift")
            .PersistKeysToAzureBlobStorage(keyBlobUri, credential)
            .ProtectKeysWithAzureKeyVault(keyVaultKeyIdentifier, credential);
    }

    private static Uri ReadHttpsUri(IConfiguration configuration, string settingName, bool requireRoot)
    {
        var value = configuration[settingName];
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment) ||
            (requireRoot && uri.AbsolutePath != "/"))
        {
            throw new InvalidOperationException($"{settingName} must be an absolute HTTPS URI without credentials or query parameters.");
        }

        return uri;
    }

    private static bool IsValidContainerName(string? name)
    {
        if (name is null || name.Length is < 3 or > 63 || name[0] == '-' || name[^1] == '-')
            return false;

        var previousWasHyphen = false;
        foreach (var character in name)
        {
            var isHyphen = character == '-';
            if (!(isHyphen || character is >= 'a' and <= 'z' || character is >= '0' and <= '9') ||
                (isHyphen && previousWasHyphen))
            {
                return false;
            }

            previousWasHyphen = isHyphen;
        }

        return true;
    }
}
