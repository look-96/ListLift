using ListLift.Application.Abstractions;

namespace ListLift.APITests.TestDoubles;

internal sealed class FakeEbayApplicationSettingsStore : IEbayApplicationSettingsStore
{
    public EbayApplicationCredentials? Credentials { get; private set; }
    public int GetCount { get; private set; }
    public int SetCount { get; private set; }
    public int DeleteCount { get; private set; }

    public Task<EbayApplicationCredentials?> GetAsync(CancellationToken cancellationToken)
    {
        GetCount++;
        return Task.FromResult(Credentials);
    }

    public Task SetAsync(EbayApplicationCredentials credentials, CancellationToken cancellationToken)
    {
        Credentials = credentials;
        SetCount++;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(CancellationToken cancellationToken)
    {
        Credentials = null;
        DeleteCount++;
        return Task.CompletedTask;
    }
}
