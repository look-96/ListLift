using System.Security.Cryptography;
using System.Text;

namespace ListLift.API.Security;

internal sealed class AdminCredentialVerifier(IConfiguration configuration) : IAdminCredentialVerifier
{
    public bool IsValid(string? credential)
    {
        var configuredHash = configuration["Admin:CredentialHash"];
        if (string.IsNullOrWhiteSpace(configuredHash) || string.IsNullOrWhiteSpace(credential))
            return false;

        byte[] expected;
        try
        {
            expected = Convert.FromHexString(configuredHash);
        }
        catch (FormatException)
        {
            return false;
        }

        if (expected.Length != SHA256.HashSizeInBytes)
            return false;

        var actual = SHA256.HashData(Encoding.UTF8.GetBytes(credential));
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
