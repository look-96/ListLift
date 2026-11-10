using ListLift.Domain.Entities;

namespace ListLift.Application.Abstractions;

public interface IListingDraftRepository
{
    Task AddAsync(ListingDraft draft, CancellationToken cancellationToken);

    Task<ListingDraft?> GetOwnedAsync(
        Guid draftId,
        string ownerUserId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ListingDraft>> GetActiveForOwnerAsync(
        string ownerUserId,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ListingDraft>> GetExpiredBatchAsync(
        DateTimeOffset nowUtc,
        int batchSize,
        CancellationToken cancellationToken);

    void DeleteRange(IEnumerable<ListingDraft> drafts);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
