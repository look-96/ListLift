using ListLift.Domain.Constants;

namespace ListLift.Domain.Entities;

public sealed class ListingCategorySelection
{
    public Guid Id { get; private set; }
    public Guid ListingDraftId { get; private set; }
    public MarketplaceChannel Channel { get; private set; }
    public string CategoryId { get; private set; } = string.Empty;
    public string CategoryName { get; private set; } = string.Empty;
    public string CategoryPath { get; private set; } = string.Empty;
    public DateTimeOffset SelectedAtUtc { get; private set; }

    private ListingCategorySelection() { }

    public static ListingCategorySelection Create(
        Guid draftId,
        MarketplaceChannel channel,
        string categoryId,
        string categoryName,
        string categoryPath,
        DateTimeOffset selectedAtUtc)
    {
        if (draftId == Guid.Empty) throw new ArgumentException("A draft ID is required.", nameof(draftId));
        if (!Enum.IsDefined(channel)) throw new ArgumentOutOfRangeException(nameof(channel));
        if (string.IsNullOrWhiteSpace(categoryId) || categoryId.Trim().Length > 64)
            throw new ArgumentException("A valid category ID is required.", nameof(categoryId));
        if (string.IsNullOrWhiteSpace(categoryName) || categoryName.Trim().Length > 200)
            throw new ArgumentException("A category name is required and must be 200 characters or fewer.", nameof(categoryName));
        if (string.IsNullOrWhiteSpace(categoryPath) || categoryPath.Trim().Length > 1000)
            throw new ArgumentException("A category path is required and must be 1,000 characters or fewer.", nameof(categoryPath));

        return new ListingCategorySelection
        {
            Id = Guid.NewGuid(),
            ListingDraftId = draftId,
            Channel = channel,
            CategoryId = categoryId.Trim(),
            CategoryName = categoryName.Trim(),
            CategoryPath = categoryPath.Trim(),
            SelectedAtUtc = selectedAtUtc
        };
    }
}
