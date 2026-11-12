using Google.Cloud.Kms.V1;
using Google.Cloud.Storage.V1;
using ListLift.Application.Abstractions;
using ListLift.Infrastructure.Photos;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ListLift.Infrastructure.GoogleCloud;

internal static class GoogleCloudStorageRegistration
{
    public static void AddListingStorage(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var photoProvider = configuration["Storage:PhotoProvider"];
        var isCloudRun = !string.IsNullOrWhiteSpace(configuration["K_SERVICE"]) ||
                         !string.IsNullOrWhiteSpace(configuration["K_REVISION"]);

        if (string.IsNullOrWhiteSpace(photoProvider))
        {
            if (isCloudRun)
                throw new InvalidOperationException("Storage:PhotoProvider must be GoogleCloudStorage when hosted on Cloud Run.");

            photoProvider = "LocalFile";
        }

        if (isCloudRun && !string.Equals(photoProvider, "GoogleCloudStorage", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cloud Run must use Google Cloud Storage for durable listing photos.");

        if (string.Equals(photoProvider, "LocalFile", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IListingPhotoStorage, PrivateFileListingPhotoStorage>();
            services.AddDataProtection().SetApplicationName("ListLift");
            return;
        }

        if (!string.Equals(photoProvider, "GoogleCloudStorage", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Storage:PhotoProvider must be LocalFile or GoogleCloudStorage.");

        var bucket = configuration["Storage:GoogleCloudBucket"];
        if (!IsValidBucketName(bucket))
            throw new InvalidOperationException("Storage:GoogleCloudBucket must be a valid Google Cloud Storage bucket name.");

        var photoPrefix = ReadPrefix(configuration["Storage:PhotoObjectPrefix"] ?? "listing-photos", "Storage:PhotoObjectPrefix");
        var keyPrefix = ReadPrefix(configuration["DataProtection:ObjectPrefix"] ?? "data-protection/listlift", "DataProtection:ObjectPrefix");
        var kmsKeyName = configuration["DataProtection:KmsKeyName"];
        if (!IsValidKmsKeyName(kmsKeyName))
            throw new InvalidOperationException("DataProtection:KmsKeyName must identify a Google Cloud KMS crypto key.");

        services.AddSingleton(StorageClient.Create());
        services.AddSingleton(KeyManagementServiceClient.Create());
        services.AddSingleton<IListingPhotoStorage>(serviceProvider =>
            new GoogleCloudStorageListingPhotoStorage(
                serviceProvider.GetRequiredService<StorageClient>(), bucket!, photoPrefix));
        services.AddSingleton<IXmlRepository>(serviceProvider =>
            new GoogleCloudStorageXmlRepository(
                serviceProvider.GetRequiredService<StorageClient>(), bucket!, keyPrefix));
        services.AddSingleton<IXmlEncryptor>(serviceProvider =>
            new GoogleCloudKmsXmlEncryptor(
                serviceProvider.GetRequiredService<KeyManagementServiceClient>(), kmsKeyName!));
        services.AddSingleton<IConfigureOptions<KeyManagementOptions>, GoogleCloudDataProtectionOptionsSetup>();
        services.AddDataProtection().SetApplicationName("ListLift");
    }

    private static bool IsValidBucketName(string? name) =>
        name is { Length: >= 3 and <= 63 } &&
        (char.IsAsciiLetterLower(name[0]) || char.IsAsciiDigit(name[0])) &&
        char.IsAsciiLetterOrDigit(name[^1]) &&
        name.All(character => char.IsAsciiLetterLower(character) || char.IsAsciiDigit(character) || character is '-' or '.') &&
        !name.Contains("..", StringComparison.Ordinal);

    private static string ReadPrefix(string value, string settingName)
    {
        var segments = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || value.StartsWith('/') || value.EndsWith('/') ||
            segments.Any(segment => segment is "." or ".." ||
                                    segment.Any(character => !(char.IsAsciiLetterOrDigit(character) || character is '-' or '_'))))
        {
            throw new InvalidOperationException($"{settingName} must contain safe alphanumeric path segments.");
        }

        return string.Join('/', segments);
    }

    private static bool IsValidKmsKeyName(string? keyName)
    {
        if (string.IsNullOrWhiteSpace(keyName))
            return false;

        var parts = keyName.Split('/');
        return parts.Length == 8 &&
               parts[0] == "projects" && IsSafeResourceName(parts[1]) &&
               parts[2] == "locations" && IsSafeResourceName(parts[3]) &&
               parts[4] == "keyRings" && IsSafeResourceName(parts[5]) &&
               parts[6] == "cryptoKeys" && IsSafeResourceName(parts[7]);
    }

    private static bool IsSafeResourceName(string value) =>
        value.Length is >= 1 and <= 63 &&
        value.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_') &&
        char.IsAsciiLetterOrDigit(value[0]) && char.IsAsciiLetterOrDigit(value[^1]);
}
