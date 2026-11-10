using ListLift.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ListLift.Infrastructure.Persistence.Configurations;

internal sealed class ListingPhotoConfiguration : IEntityTypeConfiguration<ListingPhoto>
{
    public void Configure(EntityTypeBuilder<ListingPhoto> builder)
    {
        builder.ToTable("ListingPhotos");
        builder.HasKey(photo => photo.Id);
        builder.Property(photo => photo.Id).ValueGeneratedNever();
        builder.Property(photo => photo.StorageKey).HasMaxLength(200).IsUnicode(false).IsRequired();
        builder.HasIndex(photo => photo.StorageKey).IsUnique();
        builder.Property(photo => photo.MediaType).HasMaxLength(32).IsUnicode(false).IsRequired();
        builder.Property(photo => photo.SizeBytes).IsRequired();
        builder.Property(photo => photo.SortOrder).IsRequired();
        builder.Property(photo => photo.CreatedAtUtc).IsRequired();
        builder.HasIndex(photo => new { photo.ListingDraftId, photo.SortOrder }).IsUnique();
    }
}
