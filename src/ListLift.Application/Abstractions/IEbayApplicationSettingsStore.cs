namespace ListLift.Application.Abstractions;

public interface IEbayApplicationSettingsStore
{
    Task<EbayApplicationCredentials?> GetAsync(CancellationToken cancellationToken);

    Task SetAsync(EbayApplicationCredentials credentials, CancellationToken cancellationToken);

    Task DeleteAsync(CancellationToken cancellationToken);
}
