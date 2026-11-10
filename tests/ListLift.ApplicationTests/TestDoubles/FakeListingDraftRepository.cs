using ListLift.Application.Abstractions;
using ListLift.Domain.Entities;

namespace ListLift.ApplicationTests.TestDoubles;

internal sealed class FakeListingDraftRepository : IListingDraftRepository
{
    private readonly List<ListingDraft> _drafts = [];
    public int SaveCount { get; private set; }
    public IReadOnlyList<ListingDraft> Drafts => _drafts;

    public Task AddAsync(ListingDraft draft, CancellationToken cancellationToken)
    {
        _drafts.Add(draft);
        return Task.CompletedTask;
    }

    public Task<ListingDraft?> GetOwnedAsync(Guid draftId, string ownerUserId, CancellationToken cancellationToken) =>
        Task.FromResult(_drafts.SingleOrDefault(draft => draft.Id == draftId && draft.OwnerUserId == ownerUserId));

    public Task<IReadOnlyList<ListingDraft>> GetActiveForOwnerAsync(string ownerUserId, DateTimeOffset nowUtc, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ListingDraft>>(_drafts.Where(draft => draft.OwnerUserId == ownerUserId && !draft.IsExpired(nowUtc)).ToArray());

    public Task<IReadOnlyList<ListingDraft>> GetExpiredBatchAsync(DateTimeOffset nowUtc, int batchSize, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ListingDraft>>(_drafts.Where(draft => draft.IsExpired(nowUtc)).Take(batchSize).ToArray());

    public void DeleteRange(IEnumerable<ListingDraft> drafts)
    {
        foreach (var draft in drafts)
            _drafts.Remove(draft);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}
