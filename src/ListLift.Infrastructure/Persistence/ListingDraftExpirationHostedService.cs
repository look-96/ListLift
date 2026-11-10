using ListLift.Application.ListingDrafts.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ListLift.Infrastructure.Persistence;

internal sealed class ListingDraftExpirationHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<ListingDraftExpirationHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan NormalInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MaximumRetryDelay = TimeSpan.FromMinutes(15);
    private int _consecutiveFailures;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            ExpireListingDraftsResult? result = null;
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<ExpireListingDraftsHandler>();
                result = await handler.HandleAsync(stoppingToken);
                _consecutiveFailures = result.FailedDrafts > 0 ? _consecutiveFailures + 1 : 0;

                if (result.DeletedDrafts > 0)
                    logger.LogInformation("Removed {DraftCount} expired listing drafts and their private photos.", result.DeletedDrafts);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _consecutiveFailures++;
                logger.LogError(exception, "Expired listing draft cleanup failed; the worker will retry with backoff.");
            }

            if (stoppingToken.IsCancellationRequested)
                break;

            if (_consecutiveFailures > 0)
            {
                var retryMinutes = Math.Min(Math.Pow(2, Math.Min(_consecutiveFailures - 1, 4)), MaximumRetryDelay.TotalMinutes);
                await Task.Delay(TimeSpan.FromMinutes(retryMinutes), stoppingToken);
                continue;
            }

            if (result?.MayHaveMoreExpiredDrafts == true)
                continue;

            await Task.Delay(NormalInterval, stoppingToken);
        }
    }
}
