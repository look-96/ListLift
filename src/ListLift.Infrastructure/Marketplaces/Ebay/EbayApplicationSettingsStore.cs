using ListLift.Application.Abstractions;
using ListLift.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ListLift.Infrastructure.Marketplaces.Ebay;

internal sealed class EbayApplicationSettingsStore(
    ListingDbContext dbContext,
    EbayApplicationSettingsProtector credentialsProtector,
    TimeProvider timeProvider) : IEbayApplicationSettingsStore
{
    public async Task<EbayApplicationCredentials?> GetAsync(CancellationToken cancellationToken)
    {
        var settings = await dbContext.EbayApplicationSettings.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == 1, cancellationToken);
        if (settings is null)
            return null;

        return new EbayApplicationCredentials(
            credentialsProtector.Unprotect(settings.ProtectedClientId),
            credentialsProtector.Unprotect(settings.ProtectedClientSecret));
    }

    public async Task SetAsync(EbayApplicationCredentials credentials, CancellationToken cancellationToken)
    {
        var settings = await dbContext.EbayApplicationSettings
            .SingleOrDefaultAsync(item => item.Id == 1, cancellationToken);
        if (settings is null)
        {
            settings = new EbayApplicationSettings { Id = 1, ProtectedClientId = string.Empty, ProtectedClientSecret = string.Empty };
            dbContext.EbayApplicationSettings.Add(settings);
        }

        settings.ProtectedClientId = credentialsProtector.Protect(credentials.ClientId);
        settings.ProtectedClientSecret = credentialsProtector.Protect(credentials.ClientSecret);
        settings.UpdatedAtUtc = timeProvider.GetUtcNow();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(CancellationToken cancellationToken)
    {
        var settings = await dbContext.EbayApplicationSettings
            .SingleOrDefaultAsync(item => item.Id == 1, cancellationToken);
        if (settings is null)
            return;

        dbContext.EbayApplicationSettings.Remove(settings);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
