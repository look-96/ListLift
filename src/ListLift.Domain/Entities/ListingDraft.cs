using ListLift.Domain.Constants;
using ListLift.Domain.ValueObjects;

namespace ListLift.Domain.Entities;

public sealed class ListingDraft
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

    private readonly List<ListingPhoto> _photos = [];
    private readonly List<ChannelListing> _channelListings = [];
    private readonly List<ListingCategorySelection> _categorySelections = [];

    public Guid Id { get; private set; }
    public string OwnerUserId { get; private set; } = string.Empty;
    public ListingDraftStatus Status { get; private set; } = ListingDraftStatus.Draft;
    public string ProductName { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string ConditionDescription { get; private set; } = string.Empty;
    public string? VerifiedAttributes { get; private set; }
    public string? Description { get; private set; }
    public int Quantity { get; private set; } = 1;
    public Money Price { get; private set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public bool IsExpired(DateTimeOffset nowUtc) => nowUtc >= ExpiresAtUtc;
    public IReadOnlyCollection<ListingPhoto> Photos => _photos.AsReadOnly();
    public IReadOnlyCollection<ChannelListing> ChannelListings => _channelListings.AsReadOnly();
    public IReadOnlyCollection<ListingCategorySelection> CategorySelections => _categorySelections.AsReadOnly();

    private ListingDraft() { }

    public static ListingDraft Create(
        string ownerUserId,
        string productName,
        string category,
        Money price,
        DateTimeOffset nowUtc,
        int quantity = 1)
    {
        if (string.IsNullOrWhiteSpace(ownerUserId)) throw new ArgumentException("An owner is required.", nameof(ownerUserId));
        if (string.IsNullOrWhiteSpace(productName) || productName.Trim().Length > 120)
            throw new ArgumentException("Product name is required and must be 120 characters or fewer.", nameof(productName));
        if (string.IsNullOrWhiteSpace(category) || category.Trim().Length > 100)
            throw new ArgumentException("Category is required and must be 100 characters or fewer.", nameof(category));
        ArgumentNullException.ThrowIfNull(price);
        ValidateQuantity(quantity);

        return new ListingDraft
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            ProductName = productName.Trim(),
            Category = category.Trim(),
            Quantity = quantity,
            Price = price,
            CreatedAtUtc = nowUtc,
            ExpiresAtUtc = nowUtc.Add(Lifetime)
        };
    }

    public void EnsureActive(DateTimeOffset nowUtc)
    {
        if (IsExpired(nowUtc)) throw new InvalidOperationException("This listing draft has expired.");
    }

    public void UpdateProductFacts(
        string productName,
        string category,
        string conditionDescription,
        string? verifiedAttributes,
        Money price,
        DateTimeOffset nowUtc,
        int? quantity = null)
    {
        EnsureActive(nowUtc);
        if (string.IsNullOrWhiteSpace(productName) || productName.Trim().Length > 120)
            throw new ArgumentException("Product name is required and must be 120 characters or fewer.", nameof(productName));
        if (string.IsNullOrWhiteSpace(category) || category.Trim().Length > 100)
            throw new ArgumentException("Category is required and must be 100 characters or fewer.", nameof(category));
        if (conditionDescription is not null && conditionDescription.Trim().Length > 300)
            throw new ArgumentException("Condition details must be 300 characters or fewer.", nameof(conditionDescription));
        if (verifiedAttributes is not null && verifiedAttributes.Length > 2000)
            throw new ArgumentException("Verified attributes must be 2,000 characters or fewer.", nameof(verifiedAttributes));
        ArgumentNullException.ThrowIfNull(price);
        if (quantity.HasValue) ValidateQuantity(quantity.Value);

        ProductName = productName.Trim();
        Category = category.Trim();
        ConditionDescription = conditionDescription?.Trim() ?? string.Empty;
        VerifiedAttributes = string.IsNullOrWhiteSpace(verifiedAttributes) ? null : verifiedAttributes.Trim();
        Price = price;
        if (quantity.HasValue) Quantity = quantity.Value;
        Description = null;
        Status = ListingDraftStatus.Draft;
        _categorySelections.Clear();
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity is < 1 or > 999999)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be between 1 and 999,999.");
    }

    public void SaveApprovedDescription(string description, DateTimeOffset nowUtc)
    {
        EnsureActive(nowUtc);
        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length > 4000)
            throw new ArgumentException("Description is required and must be 4,000 characters or fewer.", nameof(description));

        Description = description.Trim();
        Status = ListingDraftStatus.ReadyForReview;
    }

    public void SetDescriptionSuggestion(string description, DateTimeOffset nowUtc)
    {
        EnsureActive(nowUtc);
        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length > 4000)
            throw new ArgumentException("Description suggestion must be between 1 and 4,000 characters.", nameof(description));

        Description = description.Trim();
        Status = ListingDraftStatus.Draft;
    }

    public void AddPhoto(ListingPhoto photo, DateTimeOffset nowUtc)
    {
        EnsureActive(nowUtc);
        ArgumentNullException.ThrowIfNull(photo);
        if (photo.ListingDraftId != Id) throw new InvalidOperationException("Photo belongs to a different draft.");
        if (_photos.Any(existing => existing.Id == photo.Id)) throw new InvalidOperationException("Photo is already attached.");

        _photos.Add(photo);
    }

    public void SetMarketplaceCategory(ListingCategorySelection selection, DateTimeOffset nowUtc)
    {
        EnsureActive(nowUtc);
        ArgumentNullException.ThrowIfNull(selection);
        if (selection.ListingDraftId != Id)
            throw new InvalidOperationException("Category selection belongs to a different draft.");

        var current = _categorySelections.SingleOrDefault(item => item.Channel == selection.Channel);
        if (current is not null)
            _categorySelections.Remove(current);
        _categorySelections.Add(selection);
    }

    public void AddOrReplaceChannelListing(ChannelListing listing, DateTimeOffset nowUtc)
    {
        EnsureActive(nowUtc);
        ArgumentNullException.ThrowIfNull(listing);
        if (listing.ListingDraftId != Id) throw new InvalidOperationException("Channel listing belongs to a different draft.");
        if (Status != ListingDraftStatus.ReadyForReview || string.IsNullOrWhiteSpace(Description))
            throw new InvalidOperationException("Approve a description before preparing marketplace listings.");
        if (_photos.Count == 0)
            throw new InvalidOperationException("At least one actual item photo is required before preparing a marketplace listing.");

        var existingListing = _channelListings.SingleOrDefault(existing => existing.Channel == listing.Channel);
        if (existingListing?.Status is ChannelListingStatus.Publishing or ChannelListingStatus.Published)
        {
            throw new InvalidOperationException("A publishing or published listing cannot be silently replaced.");
        }

        if (existingListing is not null)
        {
            _channelListings.Remove(existingListing);
        }
        _channelListings.Add(listing);
    }
}
