using ListLift.Application.Abstractions;
using ListLift.Domain.Entities;
using ListLift.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ListLift.Infrastructure.Repositories;

internal sealed class ListingDraftRepository(ListingDbContext dbContext) : IListingDraftRepository
{
    public async Task AddAsync(ListingDraft draft, CancellationToken cancellationToken)
    {
        await dbContext.ListingDrafts.AddAsync(draft, cancellationToken);
    }

    public Task<ListingDraft?> GetOwnedAsync(
        Guid draftId,
        string ownerUserId,
        CancellationToken cancellationToken) =>
        dbContext.ListingDrafts
            .Include(draft => draft.Photos)
            .Include(draft => draft.ChannelListings)
            .Include(draft => draft.CategorySelections)
            .SingleOrDefaultAsync(
                draft => draft.Id == draftId && draft.OwnerUserId == ownerUserId,
                cancellationToken);

    public async Task<IReadOnlyList<ListingDraft>> GetActiveForOwnerAsync(
        string ownerUserId,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken) =>
        await dbContext.ListingDrafts
            .AsNoTracking()
            .Where(draft => draft.OwnerUserId == ownerUserId && draft.ExpiresAtUtc > nowUtc)
            .OrderByDescending(draft => draft.CreatedAtUtc)
            .Include(draft => draft.Photos)
            .Include(draft => draft.ChannelListings)
            .Include(draft => draft.CategorySelections)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ListingDraft>> GetExpiredBatchAsync(
        DateTimeOffset nowUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        if (batchSize is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be from 1 to 1,000.");

        return await dbContext.ListingDrafts
            .Where(draft => draft.ExpiresAtUtc <= nowUtc)
            .OrderBy(draft => draft.ExpiresAtUtc)
            .Include(draft => draft.Photos)
            .Include(draft => draft.ChannelListings)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public void DeleteRange(IEnumerable<ListingDraft> drafts) =>
        dbContext.ListingDrafts.RemoveRange(drafts);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
