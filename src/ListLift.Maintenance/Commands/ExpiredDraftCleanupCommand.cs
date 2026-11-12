using ListLift.Application.ListingDrafts.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ListLift.Maintenance.Commands;

internal sealed class ExpiredDraftCleanupCommand(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiredDraftCleanupCommand> logger)
{
    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        var totalDeleted = 0;

        while (true)
        {
            ExpireListingDraftsResult result;
            await using (var scope = scopeFactory.CreateAsyncScope())
            {
                var handler = scope.ServiceProvider.GetRequiredService<ExpireListingDraftsHandler>();
                result = await handler.HandleAsync(cancellationToken);
            }

            totalDeleted += result.DeletedDrafts;
            if (result.FailedDrafts > 0)
            {
                logger.LogError(
                    "Expired draft cleanup removed {DeletedDraftCount} drafts, but {FailedDraftCount} drafts failed. The job will exit unsuccessfully for retry.",
                    totalDeleted,
                    result.FailedDrafts);
                return 1;
            }

            if (!result.MayHaveMoreExpiredDrafts)
            {
                logger.LogInformation(
                    "Expired draft cleanup completed. Removed {DeletedDraftCount} drafts and their private photos.",
                    totalDeleted);
                return 0;
            }
        }
    }
}
