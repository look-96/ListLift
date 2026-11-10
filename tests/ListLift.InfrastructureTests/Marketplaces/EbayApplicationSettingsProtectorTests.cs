using System.Security.Cryptography;
using ListLift.Infrastructure.Marketplaces.Ebay;
using Microsoft.AspNetCore.DataProtection;

namespace ListLift.InfrastructureTests.Marketplaces;

public sealed class EbayApplicationSettingsProtectorTests
{
    [Fact]
    public void Protect_DoesNotExposePlaintextAndCanBeUnprotected()
    {
        const string secret = "production-secret-value";
        var protector = new EbayApplicationSettingsProtector(new EphemeralDataProtectionProvider());

        var ciphertext = protector.Protect(secret);

        Assert.NotEqual(secret, ciphertext);
        Assert.DoesNotContain(secret, ciphertext, StringComparison.Ordinal);
        Assert.Equal(secret, protector.Unprotect(ciphertext));
    }

    [Fact]
    public void Unprotect_RejectsCiphertextFromDifferentKeyRing()
    {
        var firstProtector = new EbayApplicationSettingsProtector(new EphemeralDataProtectionProvider());
        var secondProtector = new EbayApplicationSettingsProtector(new EphemeralDataProtectionProvider());
        var ciphertext = firstProtector.Protect("secret");

        Assert.Throws<CryptographicException>(() => secondProtector.Unprotect(ciphertext));
    }
}
