using ListLift.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ListLift.Infrastructure.Persistence.Configurations;

internal sealed class ChannelListingConfiguration : IEntityTypeConfiguration<ChannelListing>
{
    public void Configure(EntityTypeBuilder<ChannelListing> builder)
    {
        builder.ToTable("ChannelListings");
        builder.HasKey(listing => listing.Id);
        builder.Property(listing => listing.Id).ValueGeneratedNever();
        builder.Property(listing => listing.Channel).HasConversion<int>().IsRequired();
        builder.Property(listing => listing.Status).HasConversion<int>().IsRequired();
        builder.Property(listing => listing.ExternalListingId).HasMaxLength(128);
        builder.Property(listing => listing.SafeErrorCode).HasMaxLength(64).IsUnicode(false);
        builder.Property(listing => listing.UpdatedAtUtc).IsRequired();
        builder.HasIndex(listing => new { listing.ListingDraftId, listing.Channel }).IsUnique();
    }
}
