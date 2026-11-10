using ListLift.Application.Abstractions;
using ListLift.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ListLift.Application.ListingDrafts.Commands;

public sealed class ExpireListingDraftsHandler(
    IListingDraftRepository repository,
    IListingPhotoStorage photoStorage,
    IClock clock,
    ILogger<ExpireListingDraftsHandler> logger)
{
    private const int BatchSize = 100;

    public async Task<ExpireListingDraftsResult> HandleAsync(CancellationToken cancellationToken)
    {
        var expiredDrafts = await repository.GetExpiredBatchAsync(clock.UtcNow, BatchSize, cancellationToken);
        if (expiredDrafts.Count == 0)
            return new ExpireListingDraftsResult(0, 0, false);

        var draftsReadyForDeletion = new List<ListingDraft>(expiredDrafts.Count);
        var failedDrafts = 0;

        foreach (var draft in expiredDrafts)
        {
            try
            {
                foreach (var photo in draft.Photos)
                    await photoStorage.DeleteAsync(photo.StorageKey, cancellationToken);

                draftsReadyForDeletion.Add(draft);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                failedDrafts++;
                logger.LogWarning(exception, "Cleanup of one expired listing draft failed; it will be retried.");
            }
        }

        if (draftsReadyForDeletion.Count > 0)
        {
            repository.DeleteRange(draftsReadyForDeletion);
            await repository.SaveChangesAsync(cancellationToken);
        }

        return new ExpireListingDraftsResult(
            draftsReadyForDeletion.Count,
            failedDrafts,
            expiredDrafts.Count == BatchSize);
    }
}
