using ListLift.Domain.Constants;
using ListLift.Domain.Entities;
using ListLift.Domain.ValueObjects;

namespace ListLift.DomainTests.Entities;

public sealed class ListingDraftTests
{
    [Fact]
    public void Create_TrimsTextAndSetsTwentyFourHourExpiry()
    {
        var now = new DateTimeOffset(2026, 11, 10, 12, 0, 0, TimeSpan.Zero);

        var draft = ListingDraft.Create("seller-1", "  Camera  ", "  Electronics ", Money.Create(10m, "gbp"), now);

        Assert.Equal("Camera", draft.ProductName);
        Assert.Equal("Electronics", draft.Category);
        Assert.Equal("GBP", draft.Price.CurrencyCode);
        Assert.Equal(1, draft.Quantity);
        Assert.Equal(now, draft.CreatedAtUtc);
        Assert.Equal(now.AddHours(24), draft.ExpiresAtUtc);
    }

    [Fact]
    public void IsExpired_ExpiresAtTheDeadline()
    {
        var now = DateTimeOffset.UtcNow;
        var draft = ListingDraft.Create("seller-1", "Camera", "Electronics", Money.Create(10m, "GBP"), now);

        Assert.False(draft.IsExpired(draft.ExpiresAtUtc.AddTicks(-1)));
        Assert.True(draft.IsExpired(draft.ExpiresAtUtc));
        Assert.Throws<InvalidOperationException>(() => draft.EnsureActive(draft.ExpiresAtUtc));
    }

    [Fact]
    public void SaveApprovedDescription_TrimsTextAndMovesToReviewStatus()
    {
        var now = DateTimeOffset.UtcNow;
        var draft = ListingDraft.Create("seller-1", "Camera", "Electronics", Money.Create(10m, "GBP"), now);

        draft.SaveApprovedDescription("  Fully working camera.  ", now);

        Assert.Equal("Fully working camera.", draft.Description);
        Assert.Equal(ListingDraftStatus.ReadyForReview, draft.Status);
    }

    [Fact]
    public void Create_RejectsMissingOwner()
    {
        Assert.Throws<ArgumentException>(() => ListingDraft.Create(" ", "Camera", "Electronics", Money.Create(10m, "GBP"), DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000000)]
    public void Create_RejectsQuantityOutsideSupportedRange(int quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ListingDraft.Create(
            "seller-1", "Camera", "Electronics", Money.Create(10m, "GBP"), DateTimeOffset.UtcNow, quantity));
    }

    [Fact]
    public void UpdateProductFacts_UpdatesQuantityWhenProvided()
    {
        var now = DateTimeOffset.UtcNow;
        var draft = ListingDraft.Create("seller-1", "Camera", "Electronics", Money.Create(10m, "GBP"), now);

        draft.UpdateProductFacts("Camera", "Electronics", "Good", null, Money.Create(10m, "GBP"), now, 4);

        Assert.Equal(4, draft.Quantity);
    }
}
