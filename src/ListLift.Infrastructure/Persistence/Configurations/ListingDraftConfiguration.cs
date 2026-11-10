using ListLift.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ListLift.Infrastructure.Persistence.Configurations;

internal sealed class ListingDraftConfiguration : IEntityTypeConfiguration<ListingDraft>
{
    public void Configure(EntityTypeBuilder<ListingDraft> builder)
    {
        builder.ToTable("ListingDrafts");
        builder.HasKey(draft => draft.Id);
        builder.Property(draft => draft.Id).ValueGeneratedNever();

        builder.Property(draft => draft.OwnerUserId).HasMaxLength(128).IsRequired();
        builder.Property(draft => draft.ProductName).HasMaxLength(120).IsRequired();
        builder.Property(draft => draft.Category).HasMaxLength(100).IsRequired();
        builder.Property(draft => draft.ConditionDescription).HasMaxLength(300).IsRequired();
        builder.Property(draft => draft.VerifiedAttributes).HasMaxLength(2000);
        builder.Property(draft => draft.Description).HasMaxLength(4000);
        builder.Property(draft => draft.Quantity).HasDefaultValue(1).IsRequired();
        builder.Property(draft => draft.Status).HasConversion<int>().IsRequired();
        builder.Property(draft => draft.CreatedAtUtc).IsRequired();
        builder.Property(draft => draft.ExpiresAtUtc).IsRequired();

        builder.HasIndex(draft => draft.ExpiresAtUtc);
        builder.HasIndex(draft => new { draft.OwnerUserId, draft.ExpiresAtUtc });

        builder.OwnsOne(draft => draft.Price, price =>
        {
            price.Property(money => money.Amount)
                .HasColumnName("PriceAmount")
                .HasPrecision(18, 2)
                .IsRequired();
            price.Property(money => money.CurrencyCode)
                .HasColumnName("CurrencyCode")
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsRequired();
        });

        builder.HasMany(draft => draft.Photos)
            .WithOne()
            .HasForeignKey(photo => photo.ListingDraftId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(draft => draft.Photos)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(draft => draft.ChannelListings)
            .WithOne()
            .HasForeignKey(listing => listing.ListingDraftId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(draft => draft.ChannelListings)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(draft => draft.CategorySelections)
            .WithOne()
            .HasForeignKey(selection => selection.ListingDraftId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(draft => draft.CategorySelections)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
