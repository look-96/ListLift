using System.Security.Cryptography;
using System.Text;
using ListLift.API.Security;
using Microsoft.Extensions.Configuration;

namespace ListLift.APITests.Security;

public sealed class AdminCredentialVerifierTests
{
    [Fact]
    public void IsValid_ReturnsTrueForMatchingCredentialHash()
    {
        const string credential = "high-entropy-admin-key";
        var verifier = CreateVerifier(Hash(credential));

        Assert.True(verifier.IsValid(credential));
    }

    [Fact]
    public void IsValid_ReturnsFalseForWrongCredential()
    {
        var verifier = CreateVerifier(Hash("expected-key"));

        Assert.False(verifier.IsValid("wrong-key"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_ReturnsFalseForMissingCredential(string? credential)
    {
        var verifier = CreateVerifier(Hash("expected-key"));

        Assert.False(verifier.IsValid(credential));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-hex-hash")]
    [InlineData("ABCD")]
    public void IsValid_ReturnsFalseForMissingOrInvalidConfiguredHash(string? configuredHash)
    {
        var verifier = CreateVerifier(configuredHash);

        Assert.False(verifier.IsValid("some-key"));
    }

    private static AdminCredentialVerifier CreateVerifier(string? hash)
    {
        var settings = new Dictionary<string, string?> { ["Admin:CredentialHash"] = hash };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new AdminCredentialVerifier(configuration);
    }

    private static string Hash(string credential) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credential)));
}
