using ListLift.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ListLift.Infrastructure.Persistence.Configurations;

internal sealed class ListingCategorySelectionConfiguration : IEntityTypeConfiguration<ListingCategorySelection>
{
    public void Configure(EntityTypeBuilder<ListingCategorySelection> builder)
    {
        builder.ToTable("ListingCategorySelections");
        builder.HasKey(selection => selection.Id);
        builder.Property(selection => selection.Id).ValueGeneratedNever();
        builder.Property(selection => selection.Channel).HasConversion<int>().IsRequired();
        builder.Property(selection => selection.CategoryId).HasMaxLength(64).IsRequired();
        builder.Property(selection => selection.CategoryName).HasMaxLength(200).IsRequired();
        builder.Property(selection => selection.CategoryPath).HasMaxLength(1000).IsRequired();
        builder.Property(selection => selection.SelectedAtUtc).IsRequired();
        builder.HasIndex(selection => new { selection.ListingDraftId, selection.Channel }).IsUnique();
    }
}
