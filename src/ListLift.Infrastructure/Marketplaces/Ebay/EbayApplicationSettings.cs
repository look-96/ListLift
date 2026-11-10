namespace ListLift.Infrastructure.Marketplaces.Ebay;

internal sealed class EbayApplicationSettings
{
    public int Id { get; set; }

    public required string ProtectedClientId { get; set; }

    public required string ProtectedClientSecret { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
