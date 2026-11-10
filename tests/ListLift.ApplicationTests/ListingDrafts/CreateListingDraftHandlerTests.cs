using ListLift.Application.ListingDrafts.Commands;
using ListLift.ApplicationTests.TestDoubles;
using Xunit;

namespace ListLift.ApplicationTests.ListingDrafts;

public sealed class CreateListingDraftHandlerTests
{
    [Fact]
    public async Task HandleAsync_UsesCurrentUserAndClockAndSavesDraft()
    {
        var now = new DateTimeOffset(2026, 11, 10, 12, 0, 0, TimeSpan.Zero);
        var repository = new FakeListingDraftRepository();
        var handler = new CreateListingDraftHandler(repository, new FakeCurrentUser("owner-1"), new FakeClock(now));

        var draftId = await handler.HandleAsync(new CreateListingDraftCommand(" Camera ", " Electronics ", 24.99m, "GBP"), CancellationToken.None);

        var draft = Assert.Single(repository.Drafts);
        Assert.Equal(draftId, draft.Id);
        Assert.Equal("owner-1", draft.OwnerUserId);
        Assert.Equal(now.AddHours(24), draft.ExpiresAtUtc);
        Assert.Equal(24.99m, draft.Price.Amount);
        Assert.Equal("GBP", draft.Price.CurrencyCode);
        Assert.Equal(1, draft.Quantity);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_UsesRequestedQuantity()
    {
        var repository = new FakeListingDraftRepository();
        var handler = new CreateListingDraftHandler(repository, new FakeCurrentUser("owner-1"), new FakeClock(DateTimeOffset.UtcNow));

        await handler.HandleAsync(new CreateListingDraftCommand("Camera", "Electronics", 24.99m, "GBP", 5), CancellationToken.None);

        Assert.Equal(5, Assert.Single(repository.Drafts).Quantity);
    }

    [Fact]
    public async Task HandleAsync_InvalidPriceDoesNotPersistDraft()
    {
        var repository = new FakeListingDraftRepository();
        var handler = new CreateListingDraftHandler(repository, new FakeCurrentUser("owner-1"), new FakeClock(DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(
            () => handler.HandleAsync(new CreateListingDraftCommand("Camera", "Electronics", 0m, "GBP"), CancellationToken.None));

        Assert.Empty(repository.Drafts);
        Assert.Equal(0, repository.SaveCount);
    }
}
