using Microsoft.AspNetCore.DataProtection;

namespace ListLift.Infrastructure.Marketplaces.Ebay;

internal sealed class EbayApplicationSettingsProtector
{
    private readonly IDataProtector _protector;

    public EbayApplicationSettingsProtector(IDataProtectionProvider protectionProvider)
    {
        _protector = protectionProvider.CreateProtector("ListLift.EbayApplicationCredentials.v1");
    }

    public string Protect(string value) => _protector.Protect(value);

    public string Unprotect(string protectedValue) => _protector.Unprotect(protectedValue);
}
